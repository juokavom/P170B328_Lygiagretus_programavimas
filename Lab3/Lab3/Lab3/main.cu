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


//__global__ void run_on_gpu(Items* data, Items* results, int* size);
//__device__ void execute();

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

int main() {
	int gijuKiekis = 7;
	string fileName = "Data/IFF8-12_AkramasJ_L1_dat_1.txt";	
	//---RAM kintamieji
	Items *items = readItems(fileName);
	cout << items->maxCharSize();
	//string *results = new Items();
	int size = items->size();
	int count = 0;
	//---VRAM kintamieji
	Items *cuda_items, *cuda_results;
	int *cuda_size, *cuda_count;
	//---
	/*
	cudaMalloc(&cuda_items, sizeof(Items));
	cudaMalloc(&cuda_results, sizeof(Items));
	cudaMalloc(&cuda_size, sizeof(int));
	cudaMalloc(&cuda_count, sizeof(int));
	//---
	cudaMemcpy(cuda_items, items, sizeof(Items), cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_results, results, sizeof(Items), cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_size, &size, sizeof(int), cudaMemcpyHostToDevice);
	cudaMemcpy(cuda_count, &count, sizeof(int), cudaMemcpyHostToDevice);
	//---
	run_on_gpu << <1, gijuKiekis >> > (cuda_items, cuda_results, cuda_size); //Paleidzia gijas
	//---
	cudaDeviceSynchronize(); //Palaukti visu giju
	//---
	cudaMemcpy(results, cuda_results, sizeof(Items), cudaMemcpyDeviceToHost);
	//---

	//Print results;

	//---
	delete(items);
	delete(results);
	cudaFree(cuda_items);
	cudaFree(cuda_results);
	cudaFree(cuda_size);
	*/
	//---
	cout << "Finished" << endl;
}

/*
__global__ void run_on_gpu(Items *data, Items *results, int *size) {
	int slice_size = *size / blockDim.x;
	//---
	int start_index = slice_size * threadIdx.x;
	int end_index = (threadIdx.x == blockDim.x - 1)? *size : slice_size * (threadIdx.x + 1);
	//---

	execute();
}

__device__ void execute() {
	printf("%s: first\n");
	printf("%s: second\n");
	printf("%s: third\n");
}
*/