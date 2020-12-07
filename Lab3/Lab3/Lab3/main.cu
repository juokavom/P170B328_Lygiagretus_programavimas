#include "cuda_runtime.h"
#include <cuda.h>
#include <cstdio>
#include <fstream>
#include <iostream>
#include <string>
#include <iomanip>
#include <vector>
#include "device_launch_parameters.h"

using namespace std; 



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
		return Title.size() + 1 + to_string(calculateValue()).size();
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

	int maxCharSize(){
		int max = ItemArray[0].outputSize();
		for (int i = 1; i < size(); i++) {
			int isize = ItemArray[i].outputSize();
			max = isize > max ? isize : max;
		}
		return max + 1; //FORMATAS: 'TITLE-value '
	}

	void parseData(string* title, int *quantity, float *price) {
		for (int i = 0; i < size(); i++) {
			title[i] = ItemArray[i].Title;
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

__global__ void run_on_gpu(int* quantity, float* price, char* results, int* size, unsigned int* count);
__device__ float calculateValue(int quantity, float price);

int main() {
	int gijuKiekis = 7;
	string fileName = "Data/IFF8-12_AkramasJ_L1_dat_1.txt";	
	//---RAM kintamieji
	Items *items = readItems(fileName);
	int sector_size = items->maxCharSize();
	int size = items->size();
	//---
	string title[30];
	int quantity[30];
	float price[30];
	items->parseData(title, quantity, price);
	int resultSize = sizeof(char) * sector_size * size;
	auto *results = malloc(resultSize);
	unsigned int count = 0;
	//---VRAM kintamieji
	int* cuda_quantity;
	float* cuda_price;
	char *cuda_results;
	int* cuda_size;
	unsigned int *cuda_count;
	//---
	cudaMalloc(&cuda_quantity, sizeof(int)*size);
	cudaMalloc(&cuda_price, sizeof(float)*size);
	cudaMalloc(&cuda_results, resultSize);
	cudaMalloc(&cuda_size, sizeof(int));
	cudaMalloc(&cuda_count, sizeof(unsigned int));
	//---
	cudaMemcpy(cuda_quantity, quantity, sizeof(int) * size, cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_price, price, sizeof(float) * size, cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_results, results, resultSize, cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_size, &size, sizeof(int), cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_count, &count, sizeof(unsigned int), cudaMemcpyHostToDevice);
	//---
	run_on_gpu << <1, gijuKiekis >> > (cuda_quantity, cuda_price, cuda_results, cuda_size, cuda_count); //Paleidzia gijas
	//---
	cudaDeviceSynchronize(); //Palaukti visu giju
	//---
	cudaMemcpy(&count, cuda_count, sizeof(unsigned int), cudaMemcpyDeviceToHost);
	//---

	cout << count << endl;
	//Print results;

	//---
	delete(items);
	free(results);
	cudaFree(cuda_quantity);
	cudaFree(cuda_price);
	cudaFree(cuda_results);
	cudaFree(cuda_size);
	cudaFree(cuda_count);
	//---
	cout << "Finished" << endl;
}

__global__ void run_on_gpu(int* quantity, float* price, char *results, int *size, unsigned int *count) {
	int slice_size = *size / blockDim.x;
	//---
	int start_index = slice_size * threadIdx.x;
	int end_index = (threadIdx.x == blockDim.x - 1)? *size : slice_size * (threadIdx.x + 1);
	//---
	for (int i = start_index; i < end_index; i++) {
		float result = calculateValue(quantity[i], price[i]);
		if (result > 0.5f) {
			atomicAdd(count, 1);
		}
	}
}

__device__ float calculateValue(int quantity, float price) {
	//---
	/*
	vector<char> bytes(Title.begin(), Title.end());
	int stringValues = 0;
	for (char i : bytes) {
		stringValues += i;
	}
	int temp = stringValues ^ Quantity;
	float finalV = temp * Price;
	//---

	return finalV;
	*/
	return 0.6f;
}