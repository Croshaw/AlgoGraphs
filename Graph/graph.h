#include <iostream>
#include <list>
#include <map>
#include <hash_set>

struct Node {
	std::string name;
};

struct Edge {
	Node* begin;
	Node* end;
	int weight;
	short direction = 0;
};

class Graph
{
private:
	std::hash_set<Node*> nodes;
	std::list<Edge*> edges;
	bool containsEdge(Node* begin, Node* end);

public:
	Graph();
	~Graph();
	std::hash_set<Node*> getNodes();
	std::list<Edge*> getEdges();
	void addNode(std::string name);
	void addNode(Node* node);
	void removeNode(Node* node);
	void addEdge(Node* begin, Node* end, int weight = 1);
	void removeEdge(Node* begin, Node* end);
	Edge* findEdge(Node* begin, Node* end);
	void changeDirection(Node* begin, Node* end, int direction);
};