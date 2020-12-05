#include "cuda_runtime.h"
#include <cuda.h>
#include <cstdio>
//#include "nlohmann.hpp"
#include <fstream>
#include <iostream>
#include <stdio.h>
#include <string>
#include "device_launch_parameters.h"

using namespace std;
//using json = nlohmann::json;

__global__ void run_on_gpu();
__device__ void execute(const char* name);

class Item {
public:
    string Title;
    int Quantity;
    float Price;

    Item() {}

    Item(string title, int quantity, float price) {
        this->Title = title;
        this->Quantity = quantity;
        this->Price = price;
    }

    string ToString() {
        char buff[100];
        snprintf(buff, sizeof(buff), "|%-20s|%-8d|%-7.2f", Title.c_str(), Quantity, Price);
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

int main() {
    string line;
    ifstream myfile("Data/IFF8-12_AkramasJ_L1_dat_1.json");
    if (myfile.is_open())
    {
        while (getline(myfile, line))
        {
            cout << line << '\n';
        }
        myfile.close();
    }
    else cout << "Unable to open file";
    /*
    run_on_gpu << <1, 2 >> > ();
    cudaDeviceSynchronize();
    */
    cout << "Finished" << endl;
}

__global__ void run_on_gpu() {
    const char* name;
    if (threadIdx.x == 0) {
        name = "Thread 1";
    }
    else {
        name = "Thread 2";
    }
    execute(name);
}

__device__ void execute(const char* name) {
    printf("%s: first\n", name);
    printf("%s: second\n", name);
    printf("%s: third\n", name);
}
