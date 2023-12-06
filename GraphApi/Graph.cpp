#include <iostream>
#include "graph.h"

int main()
{
    std::cout << "Hello World!\n";
}

Graph::Graph()
{
}

Graph::~Graph()
{
    for (Node* node : nodes) {
        removeNode(node);
    }
}

std::hash_set<Node*> Graph::getNodes()
{
    return this->nodes;
}

std::list<Edge*> Graph::getEdges()
{
    return this->edges;
}

void Graph::addNode(std::string name)
{
    Node* newNode = new Node();
    newNode->name = name;
    addNode(newNode);
}

void Graph::addNode(Node* node)
{
    if (nodes.count(node) == 0) {
        nodes.insert(node);
    }
    else
    {
        throw "Node already exist";
    }
}

void Graph::removeNode(Node* node)
{
    if (nodes.count(node) == 1) {
        for (Edge* edge : edges) {
            if (edge->begin == node || edge->end == node) {
                edges.remove(edge);
                delete edge;
            }
        }
        nodes.erase(node);
        delete node;
    }
    else
    {
        throw "Node does not exist";
    }
}

void Graph::addEdge(Node* begin, Node* end, int weight)
{
    if (!containsEdge(begin, end)) {
        Edge* newEdge = new Edge();
        newEdge->begin = begin;
        newEdge->end = end;
        newEdge->weight = weight;
        edges.push_back(newEdge);
    }
    else
    {
        throw "Edge already exist";
    }
}

void Graph::removeEdge(Node* begin, Node* end)
{
    if (containsEdge(begin, end)) {
        for (Edge* edge : edges) {
            if (edge->begin == begin && edge->end == end) {
                edges.remove(edge);
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

Edge* Graph::findEdge(Node* begin, Node* end)
{
    for (Edge* edge : edges) {
        if (edge->begin == begin && edge->end == end) {
            return edge;
        }
    }
    return nullptr;
}

void Graph::changeDirection(Node* begin, Node* end, int direction)
{
    Edge* edge = findEdge(begin, end);
    edge->direction = direction;
}

bool Graph::containsEdge(Node* begin, Node* end)
{
    for (Edge* edge : edges) {
        if (edge->begin == begin && edge->end == end) {
            return true;
        }
    }
    return false;
}
