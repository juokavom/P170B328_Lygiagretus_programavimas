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


__global__ void run_on_gpu();
__device__ void execute(const char* name);

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
};

class Items {
public:
	Item ItemArray[30];

	int size() {
		return sizeof(ItemArray) / sizeof(ItemArray[0]);
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
	auto items = readItems(fileName);
	cout << sizeof(Items) << endl;
	/*
	run_on_gpu << <1, gijuKiekis >> > (); //Paleidzia gijas
	cudaDeviceSynchronize(); //Palaukti visu giju
	*/
	//Isvesti rezultatus


	delete(items);
	cout << "Finished" << endl;
}

__global__ void run_on_gpu() {
	const char* name;
	
	execute(name);
}

__device__ void execute(const char* name) {
	printf("%s: first\n", name);
	printf("%s: second\n", name);
	printf("%s: third\n", name);
}
