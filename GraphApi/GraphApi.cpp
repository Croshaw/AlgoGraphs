#include "pch.h"

#include "GraphApi.h"
#include <set>


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
	auto nodes = graph->getNodes();
	auto edges = graph->getEdges();
	int size = nodes->Count;
	auto adjacencyMatrix = gcnew Generic::List<Generic::List<int>^>();
	for (int i = 0; i < size; ++i) {
		auto row = gcnew Generic::List<int>();
		for (int i = 0; i < size; i++) {
			row->Add(0);
		}
		adjacencyMatrix->Add(row);
	}
	for each (Edge ^ edge in edges) {
		int beginIndex = nodes->IndexOf(edge->begin);
		int endIndex = nodes->IndexOf(edge->end);
		int val = edge->weight == 0 ? 1 : edge->weight;
		if (edge->direction == 0 || edge->direction == 2) {
			auto tL = adjacencyMatrix[beginIndex];
			tL[endIndex] += val;
			tL = adjacencyMatrix[endIndex];
			tL[beginIndex] += val;
		}
		else if (edge->direction == -1) {
			auto tL = adjacencyMatrix[endIndex];
			tL[beginIndex] += val;
		} if (edge->direction == 1) {
			auto tL = adjacencyMatrix[beginIndex];
			tL[endIndex] += val;
		}
	}
	return adjacencyMatrix;
}

Generic::List<GraphApi::Node^>^ GraphApi::GraphHelper::adjacencyList(Node^ node) {
	auto adjacentNodes = gcnew Generic::List<Node^>();
	auto nodes = graph->getNodes();
	auto edges = graph->getEdges();
	for each (Edge ^ edge in edges) {
		if (edge->begin == node && (edge->direction == 1 || edge->direction == 0 || edge->direction == 2)) {
			adjacentNodes->Add(edge->end);
		}
		else if (edge->end == node && (edge->direction == -1 || edge->direction == 0 || edge->direction == 2)) {
			adjacentNodes->Add(edge->begin);
		}
	}
	return adjacentNodes;
}

Generic::List<Generic::List<short>^>^ GraphApi::GraphHelper::incidenceMatrix() {
	auto nodes = graph->getNodes();
	auto edges = graph->getEdges();
	auto incidenceMatrix = gcnew Generic::List<Generic::List<short>^>();

	for each (Node ^ node in nodes) {
		auto row = gcnew Generic::List<short>();
		for (int i = 0; i < edges->Count; i++) {
			row->Add(0);
		}
		incidenceMatrix->Add(row);
	}
	short id = 0;
	for each (Edge ^ edge in edges) {
		int beginIndex = nodes->IndexOf(edge->begin);
		int endIndex = nodes->IndexOf(edge->end);
		int val = edge->weight == 0 ? 1 : edge->weight;
		if (edge->direction == 0 || edge->direction == 1 || edge->direction == -1) {
			auto t = incidenceMatrix[beginIndex];
			t[id] = val * (edge->direction == 1 ? 1 : -1);
			t = incidenceMatrix[endIndex];
			t[id] = val * (edge->direction == -1 ? 1 : -1);
		}
		else if (edge->direction == 2) {
			for each (auto tempList in incidenceMatrix) {
				tempList->Add(0);
			}
			auto t = incidenceMatrix[beginIndex];
			t[id] = -val;
			t[id+1] = val;
			t = incidenceMatrix[endIndex];
			t[id] = val;
			t[id+1] = -val;
			id++;
		}
		id++;
	}

	return incidenceMatrix;
}

Generic::List<Generic::List<int>^>^ GraphApi::GraphHelper::weightMatrix() {
	auto weightMatrix = gcnew Generic::List<Generic::List<int>^>();
	auto nodes = graph->getNodes();
	auto edges = graph->getEdges();
	int numNodes = nodes->Count;

	for (int i = 0; i < numNodes; i++) {
		auto row = gcnew Generic::List<int>();
		for (int i = 0; i < numNodes; i++)
			row->Add(0);
		weightMatrix->Add(row);
	}

	for each (Edge ^ edge in edges) {
		int beginIndex = nodes->IndexOf(edge->begin);
		int endIndex = nodes->IndexOf(edge->end);

		if (edge->direction == 0 || edge->direction == 2) {
			auto t = weightMatrix[beginIndex];
			t[endIndex] = edge->weight;
			t = weightMatrix[endIndex];
			t[beginIndex] = edge->weight;
		}
		else if (edge->direction == 1 || edge->direction == -1) {
			auto t = weightMatrix[beginIndex];
			t[endIndex] = edge->direction == -1 ? -1 : edge->weight;
			t = weightMatrix[endIndex];
			t[beginIndex] = edge->direction == -1 ? edge->weight : -1;
		}
	}

	return weightMatrix;
}

Generic::List<GraphApi::Edge^>^ GraphApi::GraphHelper::minSpanningTree() {
	System::Collections::Generic::List<Edge^>^ mst = gcnew System::Collections::Generic::List<Edge^>();
	auto nodes = graph->getNodes();
	auto edges = graph->getEdges();
	if (nodes->Count == 0 || edges->Count == 0)
		return mst;

	Generic::List<GraphApi::Node^>^ remainingNodes = gcnew Generic::List<GraphApi::Node^>(nodes);
	Node^ startNode = nodes[0];
	remainingNodes->Remove(startNode);

	while (remainingNodes->Count != 0) {
		Edge^ minEdge = nullptr;
		int minWeight = std::numeric_limits<int>::max();

		for each (Edge ^ edge in edges) {
			// Проверяем, принадлежит ли начальная или конечная точка ребра оставшимся узлам
			if (remainingNodes->Contains(edge->begin) ^ remainingNodes->Contains(edge->end)) {
				if (edge->weight < minWeight) {
					minWeight = edge->weight;
					minEdge = edge;
				}
			}
		}

		if (minEdge != nullptr) {
			mst->Add(minEdge);
			remainingNodes->Remove(minEdge->begin);
			remainingNodes->Remove(minEdge->end);
		}
	}

	return mst;
}

int GraphApi::GraphHelper::getPathLength()
{
	return getPathLength(graph->getEdges());
}

int GraphApi::GraphHelper::getPathLength(Generic::List<Edge^>^ edges)
{
	int len = 0;
	for each (auto edge in edges) {
		len += edge->weight;
	}
	return len;
}
