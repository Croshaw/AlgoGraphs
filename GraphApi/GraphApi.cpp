#include "pch.h"

#include "GraphApi.h"


bool GraphApi::Graph::containsEdge(Node^ begin, Node^ end)
{
	for (int i = 0; i < edges->Count; i++) {
		if (edges[i]->begin == begin && edges[i]->end == end) {
			return true;
		}
	}
	return false;
}


GraphApi::Graph::Graph()
{
	nodes = gcnew Generic::List<Node^>();
	edges = gcnew Generic::List<Edge^>();
}

GraphApi::Graph::~Graph()
{
	for (int i = 0; i < nodes->Count; i++) {
		Node^ node = nodes[i];
		removeNode(node);
	}
}

Generic::List<GraphApi::Node^>^ GraphApi::Graph::getNodes()
{
	return nodes;
}

Generic::List<GraphApi::Edge^>^ GraphApi::Graph::getEdges()
{
	return edges;
}

void GraphApi::Graph::addNode(System::String^ name)
{
	Node^ newNode = gcnew Node();
	newNode->name = name;
	addNode(newNode);
}

void GraphApi::Graph::addNode(Node^ node)
{
	if (!nodes->Contains(node)) {
		nodes->Add(node);
	}
	else {
		throw "Node already exist";
	}
}

void GraphApi::Graph::removeNode(Node^ node)
{
	if (nodes->Contains(node)) {
		for (int i = 0; i < edges->Count; i++) {
			Edge^ edge = edges[i];
			if (edge->begin == node || edge->end == node) {
				edges->Remove(edge);
				delete edge;
			}
		}
		nodes->Remove(node);
		delete node;
	}
	else
	{
		throw "Node does not exist";
	}
}

void GraphApi::Graph::addEdge(Node^ begin, Node^ end)
{
	addEdge(begin, end, 1);
}

void GraphApi::Graph::addEdge(Node^ begin, Node^ end, int weight)
{
	if (!containsEdge(begin, end)) {
		Edge^ newEdge = gcnew Edge();
		newEdge->begin = begin;
		newEdge->end = end;
		newEdge->weight = weight;
		edges->Add(newEdge);
	}
	else
	{
		throw "Edge already exist";
	}
}

void GraphApi::Graph::removeEdge(Node^ begin, Node^ end)
{
	if (containsEdge(begin, end)) {
		for (int i = 0; i < edges->Count; i++) {
			Edge^ edge = edges[i];
			if (edge->begin == begin && edge->end == end) {
				edges->Remove(edge);
				delete edge;
				break;
			}
		}
	}
	else
	{
		throw "Edge does not exist";
	}
}

GraphApi::Edge^ GraphApi::Graph::findEdge(Node^ begin, Node^ end)
{
	for (int i = 0; i < edges->Count; i++) {
		Edge^ edge = edges[i];
		if (edge->begin == begin && edge->end == end) {
			return edge;
		}
	}
	return nullptr;
}

void GraphApi::Graph::changeDirection(Node^ begin, Node^ end, int direction)
{
	Edge^ edge = findEdge(begin, end);
	edge->direction = direction;
}

GraphApi::GraphHelper::GraphHelper(Graph^ graph) {
	GraphApi::GraphHelper::graph = graph;
}

Generic::List<Generic::List<int>^>^ GraphApi::GraphHelper::adjacencyMatrix() {
	auto adjacencyMatrix = gcnew Generic::List<Generic::List<int>^>();
	auto nodes = graph->getNodes();
	auto edges = graph->getEdges();
	int size = nodes->Count;
	for (int i = 0; i < size; ++i) {
		auto row = gcnew Generic::List<int>(size);
		adjacencyMatrix->Add(row);
	}
	for each (Edge ^ edge in edges) {
		int beginIndex = nodes->IndexOf(edge->begin);
		int endIndex = nodes->IndexOf(edge->end);
		auto tL = adjacencyMatrix[beginIndex];
		tL[endIndex] = edge->weight;

		if (edge->direction == 0) {
			tL[beginIndex] = edge->weight;
		}
	}
	return adjacencyMatrix;
}

Generic::List<GraphApi::Node^>^ GraphApi::GraphHelper::adjacencyList(Node^ node) {
	auto adjacentNodes = gcnew Generic::List<Node^>();
	auto nodes = graph->getNodes();
	auto edges = graph->getEdges();
	for each (Edge ^ edge in edges) {
		if (edge->begin == node) {
			adjacentNodes->Add(edge->end);
		}
		else if (edge->end == node && edge->direction == 0) {
			adjacentNodes->Add(edge->begin);
		}
	}

	return adjacentNodes;
}

Generic::List<Generic::List<short>^>^ GraphApi::GraphHelper::incidenceMatrix() {
	auto incidenceMatrix = gcnew Generic::List<Generic::List<short>^>();
	auto nodes = graph->getNodes();
	auto edges = graph->getEdges();

	for each (Node ^ node in nodes) {
		incidenceMatrix->Add(gcnew Generic::List<short>(edges->Count));
	}

	for (int i = 0; i < edges->Count; i++) {
		Edge^ edge = edges[i];
		int beginIndex = nodes->IndexOf(edge->begin);
		int endIndex = nodes->IndexOf(edge->end);

		if (beginIndex >= 0 && endIndex >= 0) {
			auto tempB = incidenceMatrix[beginIndex];
			tempB[i] = edge->direction;
			auto tempE = incidenceMatrix[endIndex];
			tempE[i] = -edge->direction;
		}
	}

	return incidenceMatrix;
}

Generic::List<Generic::List<int>^>^ GraphApi::GraphHelper::weightMatrix() {
	auto weightMatrix = gcnew Generic::List<Generic::List<int>^>();
	auto nodes = graph->getNodes();
	auto edges = graph->getEdges();
	int numNodes = nodes->Count;

	for (int i = 0; i < numNodes; i++) {
		Generic::List<int>^ row = gcnew Generic::List<int>();
		for (int j = 0; j < numNodes; j++) {
			row->Add(0);
		}
		weightMatrix->Add(row);
	}

	for each (Edge ^ edge in edges) {
		int beginIndex = nodes->IndexOf(edge->begin);
		int endIndex = nodes->IndexOf(edge->end);
		auto tempB = weightMatrix[beginIndex];
		tempB[endIndex] = edge->weight;

		if (edge->direction == 0) {
			auto tempE = weightMatrix[endIndex];
			tempE[beginIndex] = edge->weight;
		}
	}

	return weightMatrix;
}

Generic::List<GraphApi::Edge^>^ GraphApi::GraphHelper::minSpanningTree() {
	auto minSpanningTree = gcnew Generic::List<GraphApi::Edge^>();
	auto nodes = graph->getNodes();
	auto edges = graph->getEdges();


	if (nodes->Count == 0)
	{
		throw gcnew System::InvalidOperationException("Граф не содержит вершин.");
	}

	Generic::List<Node^>^ addedNodes = gcnew Generic::List<Node^>();

	Node^ startNode = nodes[0];
	addedNodes->Add(startNode);

	while (addedNodes->Count < nodes->Count)
	{
		Edge^ minEdge = nullptr;
		Node^ minDestNode = nullptr;

		for each (Edge ^ edge in edges)
		{
			if (addedNodes->Contains(edge->begin))
			{
				if (!addedNodes->Contains(edge->end))
				{
					if (minEdge == nullptr || edge->weight < minEdge->weight)
					{
						minEdge = edge;
						minDestNode = edge->end;
					}
				}
			}
		}

		if (minEdge != nullptr && minDestNode != nullptr)
		{
			minSpanningTree->Add(minEdge);

			addedNodes->Add(minDestNode);
		}
		else
		{
			throw gcnew System::InvalidOperationException("Невозможно построить минимальное остовное дерево.");
		}
	}

	return minSpanningTree;
}