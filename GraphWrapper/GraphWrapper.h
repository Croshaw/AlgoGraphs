#pragma once
#include <string>
#include <list>
#include <map>
#include <hash_set>
#include "..\GraphApi\Graph.h"

using namespace System;
using namespace Collections;

namespace GraphWrapper {
	public ref class WrapperNode {
	public:
		System::String^ name;
	};

	public ref class WrapperEdge {
	public:
		WrapperNode^ begin;
		WrapperNode^ end;
		int weight;
		short direction = 0;
	};

	public ref class GraphWrapper
	{
	private:
		Graph* graph;
		Generic::SortedSet<WrapperNode^>^ nodes;
		Generic::List<WrapperEdge^>^ edges;
		~GraphWrapper();
		Node* getNodeFromWrapperNode(WrapperNode^ node);
		void addNode(WrapperNode^ node);
	public:
		GraphWrapper();
		Generic::SortedSet<WrapperNode^>^ getNodes();
		Generic::List<WrapperEdge^>^ getEdges();
		void addNode(System::String^ name);
		void removeNode(WrapperNode^ node);
		void addEdge(WrapperNode^ begin, WrapperNode^ end, int weight);
		void removeEdge(WrapperNode^ begin, WrapperNode^ end);
		WrapperEdge^ findEdge(WrapperNode^ begin, WrapperNode^ end);
		void changeDirection(WrapperNode^ begin, WrapperNode^ end, int direction);
	};
}
