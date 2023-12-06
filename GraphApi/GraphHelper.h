#include <iostream>
#include "graph.h"

class GraphHelper
{
private:
	Graph& graph;
public:
	GraphHelper(Graph& graph);
	std::vector<std::vector<int>> adjacencyMatrix();
	std::list<Node*> adjacencyList(Node* node);
	std::vector<std::vector<short>> incidenceMatrix();
	std::vector<std::vector<int>> weightMatrix();
	std::list<Edge*> minSpanningTree();
};

