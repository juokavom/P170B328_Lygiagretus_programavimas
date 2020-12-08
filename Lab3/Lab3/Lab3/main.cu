#include "cuda_runtime.h"
#include <cuda.h>
#include <cstdio>
#include <fstream>
#include <iostream>
#include <string>
#include <iomanip>
#include <algorithm>
#include <vector>
#include "device_launch_parameters.h"
#include <thrust/host_vector.h>
#include <thrust/device_vector.h>
#include <thrust/iterator/zip_iterator.h>
#include <new>


using namespace std;
using namespace thrust;



class Item {
public:
	string Title;
	int Quantity;
	float Price;

	Item() {}

	Item(string parts[]) {
		this->Title = parts[0];
		this->Quantity = std::atoi(parts[1].c_str());
		this->Price = stof(parts[2].c_str());
	}

	string ToString() {
		char buff[100];
		snprintf(buff, sizeof(buff), "|%-20s|%-8d|%-7.2f\n", Title.c_str(), Quantity, Price);
		std::string buffAsStdStr = buff;
		return buffAsStdStr;
	}

	float calculateValue() {
		vector<char> bytes(Title.begin(), Title.end());
		int stringValues = 0;
		for (char i : bytes) {
			stringValues += i;
		}
		int temp = stringValues ^ Quantity;
		float finalV = temp * Price;
		return finalV;
	}

	int outputSize() {
		return Title.size() + 3;
	}

	string ToStringWithValue() {
		char buff[100];
		snprintf(buff, sizeof(buff), "|%s %f|\n", Title.c_str(), calculateValue());
		std::string buffAsStdStr = buff;
		return buffAsStdStr;
	}
};

class Items {
public:
	Item ItemArray[30];

	int size() {
		return sizeof(ItemArray) / sizeof(ItemArray[0]);
	}

	int maxCharSize() {
		int max = ItemArray[0].outputSize();
		for (int i = 1; i < size(); i++) {
			int isize = ItemArray[i].outputSize();
			max = isize > max ? isize : max;
		}
		return max + 1; //FORMATAS: 'TITLE-value '
	}

	void parseData(char* title, int* titleIndex, int* quantity, float* price, int* chunkSize) {
		for (int i = 0; i < size(); i++) {
			char* curTitle = &ItemArray[i].Title[0];
			titleIndex[i] = strlen(curTitle);
			for (int u = 0; u < titleIndex[i]; u++) {
				int globalIndex = i * *chunkSize + u;
				title[globalIndex] = curTitle[u];
			}
			for (int u = titleIndex[i]; u < *chunkSize; u++) {
				int globalIndex = i * *chunkSize + u;
				title[globalIndex] = ' ';
			}
			quantity[i] = ItemArray[i].Quantity;
			price[i] = ItemArray[i].Price;
		}
	}
};

Items* readItems(string file) {
	auto items = new Items();
	string s;
	ifstream ifs(file);
	std::string delimiter = ";";
	int CurrentLine = 0;
	string itemsParsed[3];
	if (ifs.is_open())
	{
		while (getline(ifs, s))
		{
			int current = 0;
			size_t pos = 0;
			string token;
			while ((pos = s.find(delimiter)) != std::string::npos) {
				token = s.substr(0, pos);
				itemsParsed[current++] = token;
				//std::cout << token << std::endl;
				s.erase(0, pos + delimiter.length());
			}
			items->ItemArray[CurrentLine++] = Item(itemsParsed);
		}
		ifs.close();
	}
	else cout << "Unable to open file";
	return items;
}

__global__ void run_on_gpu(char* title, int* titleLength, int* quantity, float* price, char* results, int* size, unsigned int* count, int* chunk);
__device__ float calculateValue(char* title, int titleLength, int quantity, float price);
__device__ char* getTitle(char* arr, int begin, int len);
__device__ void writeItem(char* results, unsigned int* count, int* chunk, char* title, int titleLength, float* result, int* res);

int main() {
	int gijuKiekis = 7;
	string fileName = "Data/IFF8-12_AkramasJ_L1_dat_1.txt";
	//---RAM kintamieji
	Items* items = readItems(fileName);
	int arrayChunkSize = items->maxCharSize();
	int size = items->size();
	//---
	char* title = (char*)malloc(sizeof(char) * arrayChunkSize * size);
	int* titleLength = (int*)malloc(sizeof(int) * size);
	int* quantity = (int*)malloc(sizeof(int) * size);
	float* price = (float*)malloc(sizeof(float) * size);
	//---
	int sizeee = sizeof(char) * arrayChunkSize * size;
	items->parseData(title, titleLength, quantity, price, &arrayChunkSize);
	unsigned int count = 0;
	//---VRAM kintamieji
	char* cuda_title;
	int* cuda_title_length;
	int* cuda_quantity;
	float* cuda_price;
	char* cuda_results;
	int* cuda_size;
	unsigned int* cuda_count;
	int* cuda_chunk_size;
	//---
	cudaMalloc(&cuda_title, sizeof(char) * arrayChunkSize * size);
	cudaMalloc(&cuda_title_length, sizeof(int) * size);
	cudaMalloc(&cuda_quantity, sizeof(int) * size);
	cudaMalloc(&cuda_price, sizeof(float) * size);
	cudaMalloc(&cuda_results, sizeof(char) * arrayChunkSize * size);
	cudaMalloc(&cuda_size, sizeof(int));
	cudaMalloc(&cuda_count, sizeof(unsigned int));
	cudaMalloc(&cuda_chunk_size, sizeof(int));
	//---
	cudaMemcpy(cuda_title, title, sizeof(char) * arrayChunkSize * size, cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_title_length, titleLength, sizeof(int) * size, cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_quantity, quantity, sizeof(int) * size, cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_price, price, sizeof(float) * size, cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_size, &size, sizeof(int), cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_count, &count, sizeof(unsigned int), cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_chunk_size, &arrayChunkSize, sizeof(int), cudaMemcpyHostToDevice);
	//---
	run_on_gpu << <1, gijuKiekis >> > (cuda_title, cuda_title_length, cuda_quantity, cuda_price, cuda_results, cuda_size, cuda_count, cuda_chunk_size); //Paleidzia gijas
	//---
	cudaDeviceSynchronize(); //Palaukti visu giju
	//---
	cudaMemcpy(&count, cuda_count, sizeof(unsigned int), cudaMemcpyDeviceToHost);
	char* results = (char*)malloc(sizeof(char) * arrayChunkSize * count);
	cudaMemcpy(results, cuda_results, sizeof(char) * arrayChunkSize * count, cudaMemcpyDeviceToHost);
	//---
	for (int i = 0; i < sizeof(char) * arrayChunkSize * count; i++) {
		printf("%c", results[i]);
	}
	//---
	delete(items);
	free(title);
	free(titleLength);
	free(quantity);
	free(price);
	free(results);
	cudaFree(cuda_title);
	cudaFree(cuda_title_length);
	cudaFree(cuda_quantity);
	cudaFree(cuda_price);
	cudaFree(cuda_results);
	cudaFree(cuda_size);
	cudaFree(cuda_count);
	cudaFree(cuda_chunk_size);
	//---
	cout << "Finished" << endl;
}


__global__ void run_on_gpu(char* title, int* titleLength, int* quantity, float* price, char* results, int* size, unsigned int* count, int* chunk) {
	//printf("count === %d\n", *count);
	int slice_size = *size / blockDim.x;
	//---
	int start_index = slice_size * threadIdx.x;
	int end_index = (threadIdx.x == blockDim.x - 1) ? *size : slice_size * (threadIdx.x + 1);
	//---
	for (int i = start_index; i < end_index; i++) {
		//printf("thread: %d, index: %d, count === %d\n", threadIdx.x, i, *count);
		int stringIndex = *chunk * i;
		int stringLength = titleLength[i];
		char* curr_title = getTitle(title, stringIndex, stringLength);
		float result = calculateValue(curr_title, titleLength[i], quantity[i], price[i]);
		float result2 = result - (int)result;
		if (result2 > 0.5f) {
			int res = result2 * 100;
			unsigned int current_count = atomicAdd(count, 1);
			writeItem(results, &current_count, chunk, curr_title, titleLength[i], &result, &res);
			printf("current count: %u, title: %s, result: %f\n", current_count, curr_title, result);
		}
	}
}
__device__ void writeItem(char* results, unsigned int* count, int* chunk, char* title, int titleLength, float* result, int* res) {
	int current_index = (int)*count * *chunk;
	int end_index = current_index + *chunk;
	//printf("Pries\n%s\n", results);
	for (int i = 0; i < titleLength; i++) {
		char titleValue = (title[i] >= 97 && title[i] <= 122) ? title[i] - 32 : title[i];
		titleValue = (title[i] == ' ') ? '_' : titleValue;
		results[current_index] = titleValue;
		current_index++;
	}
	results[current_index] = '-';
	current_index++;
	int nr = *res / 10;
	results[current_index] = ('0' + nr);
	current_index++;
	nr = *res - nr * 10;
	results[current_index] = ('0' + nr);
	current_index++;
	for (int i = current_index; i < end_index; i++) {
		results[i] = ' ';
	}
	//printf("Po\n%s\n", results);
}
__device__ char* getTitle(char* arr, int begin, int len) {
	char* res = new char[len];
	for (int i = 0; i < len; i++) {
		res[i] = *(arr + begin + i);
	}
	return res;
}
__device__ float calculateValue(char* title, int titleLength, int quantity, float price) {
	int stringValues = 0;
	for (int i = 0; i < titleLength; i++) {
		stringValues += title[i];
	}
	int temp = stringValues ^ quantity;
	float finalV = temp * price;
	return finalV;
}