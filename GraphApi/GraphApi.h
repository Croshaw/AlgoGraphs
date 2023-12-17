#pragma once
#define _SILENCE_STDEXT_HASH_DEPRECATION_WARNINGS

using namespace System;
using namespace Collections;

namespace GraphApi {
	public ref class Node {
	public:
		System::String^ name;
	};

	public ref class Edge {
	public:
		Node^ begin;
		Node^ end;
		int weight;
		short direction = 0;
	};

	public ref class Graph
	{
	private:
		Generic::List<Node^>^ nodes;
		Generic::List<Edge^>^ edges;
		bool containsEdge(Node^ begin, Node^ end);

	public:
		Graph();
		~Graph();
		Generic::List<Node^>^ getNodes();
		Generic::List<Edge^>^ getEdges();
		void addNode(System::String^ name);
		void addNode(Node^ node);
		void removeNode(Node^ node);
		void addEdge(Node^ begin, Node^ end);
		void addEdge(Node^ begin, Node^ end, int weight);
		void removeEdge(Node^ begin, Node^ end);
		Edge^ findEdge(Node^ begin, Node^ end);
		void changeDirection(Node^ begin, Node^ end, int direction);
	};

	public ref class GraphHelper
	{
	private:
		Graph^ graph;
	public:
		GraphHelper(Graph^ graph);
		Generic::List<Generic::List<int>^>^ adjacencyMatrix();
		Generic::List<Node^>^ adjacencyList(Node^ node);
		Generic::List<Generic::List<short>^>^ incidenceMatrix();
		Generic::List<Generic::List<int>^>^ weightMatrix();
		Generic::List<Edge^>^ minSpanningTree();
	};
}
