cmake_minimum_required(VERSION 3.10)
project(perfopt)

add_library(perfopt SHARED main.cpp)

target_compile_definitions(perfopt PRIVATE EXPORT)
target_link_libraries(perfopt psapi)
