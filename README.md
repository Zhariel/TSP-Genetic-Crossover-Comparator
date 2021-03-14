# TSP-Genetic-Crossover-Comparator

A visualization of the Travelling Sales Person solved by Genetic Algorithms.

The ultimate goal of this project serves a research purpose, which is to benchmark different Crossover operators relative to the number of vertices on the graph, among other factors. 

At any given time, we can't say for sure that the algorithm has found the end-all solution for a particular problem, or not. However, we can compare how fit a generation is compared to others with the fitness rating, which is equal to the overall length of the entire tour, divided by ten thousands. 

Two operating modes are supported : unitary, and batch resolutions over a fixed number of iterations. The data is then extracted into a graph that plots the average fitness rating of each Crossover algorithm over however many cut points we desire.

Built using Godot Engine for C# Mono.


![alt text](https://i.imgur.com/Q0bRRas.png)


![alt text](https://i.imgur.com/gthSHe8.png)
