#include "pch.h"
#include <msclr/marshal_cppstd.h>

#include "GraphWrapper.h"

GraphWrapper::GraphWrapper::~GraphWrapper() {
	graph->~Graph();
}

Node* GraphWrapper::GraphWrapper::getNodeFromWrapperNode(WrapperNode^ node)
{
	for (Node* tNode : graph->getNodes()) {
		if (msclr::interop::marshal_as<System::String^>(tNode->name) == node->name)
			return tNode;
	}
	return nullptr;
}

void GraphWrapper::GraphWrapper::addNode(WrapperNode^ node)
{
	if (!nodes->Contains(node))
		nodes->Add(node);
}

GraphWrapper::GraphWrapper::GraphWrapper() {
	this->graph = new Graph();
	nodes = gcnew Generic::SortedSet<WrapperNode^>();
	edges = gcnew Generic::List<WrapperEdge^>();
}

Generic::SortedSet<GraphWrapper::WrapperNode^>^ GraphWrapper::GraphWrapper::getNodes()
{
	return nodes;
}

Generic::List<GraphWrapper::WrapperEdge^>^ GraphWrapper::GraphWrapper::getEdges()
{
	return edges;
}

void GraphWrapper::GraphWrapper::addNode(System::String^ name)
{
	std::string unmanagedString = msclr::interop::marshal_as<std::string>(name);
	graph->addNode(unmanagedString);
	WrapperNode^ node = gcnew WrapperNode();
	node->name = name;
	addNode(node);
}

void GraphWrapper::GraphWrapper::removeNode(WrapperNode^ node)
{
	Node* tNode = getNodeFromWrapperNode(node);
	if (tNode == nullptr)
		throw "Node does not exist!";
	graph->removeNode(tNode);
	for (int i = 0; i < edges->Count; i++) {
		if (edges[i]->begin == node || edges[i]->end == node) {
			edges->RemoveAt(i);
			i--;
		}
	}
	nodes->Remove(node);
}

void GraphWrapper::GraphWrapper::addEdge(WrapperNode^ begin, WrapperNode^ end, int weight)
{
	WrapperEdge^ edge = gcnew WrapperEdge();
	edge->begin = begin;
	edge->end = end;
	edge->weight = weight;
	edge->direction = 0;
	graph->addEdge(getNodeFromWrapperNode(begin), getNodeFromWrapperNode(end), weight);
}

void GraphWrapper::GraphWrapper::removeEdge(WrapperNode^ begin, WrapperNode^ end)
{
	graph->removeEdge(getNodeFromWrapperNode(begin), getNodeFromWrapperNode(end));
	WrapperEdge^ edge = findEdge(begin, end);
	if (edge != nullptr)
		edges->Remove(edge);
}

GraphWrapper::WrapperEdge^ GraphWrapper::GraphWrapper::findEdge(WrapperNode^ begin, WrapperNode^ end)
{
	for (int i = 0; i < edges->Count; i++) {
		if (edges[i]->begin == begin && edges[i]->end == end) {
			return edges[i];
		}
	}
	return nullptr;
}

void GraphWrapper::GraphWrapper::changeDirection(WrapperNode^ begin, WrapperNode^ end, int direction)
{
	graph->changeDirection(getNodeFromWrapperNode(begin), getNodeFromWrapperNode(end), direction);
	WrapperEdge^ edge = findEdge(begin, end);
	if (edge != nullptr)
		edge->direction = direction;
}