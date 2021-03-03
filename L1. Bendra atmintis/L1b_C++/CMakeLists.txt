cmake_minimum_required(VERSION 3.17)
project(P170B328_Lab1b)

set(CMAKE_CXX_STANDARD 14)
set(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -std=c++11 -fopenmp")

add_executable(P170B328_Lab1b IFF812_AkramasJ_L1b.cpp)