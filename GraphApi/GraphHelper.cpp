#include <iostream>
#include <queue>
#include "graphhelper.h"
#include <unordered_set>
#include "graph.h"


GraphHelper::GraphHelper(Graph& graph) : graph(graph) {}

std::vector<std::vector<int>> GraphHelper::adjacencyMatrix() {
    std::hash_set<Node*> nodes = graph.getNodes();
    int size = nodes.size();
    std::vector<std::vector<int>> matrix(size, std::vector<int>(size, 0));

    std::list<Edge*> edges = graph.getEdges();
    for (const auto& edge : edges) {
        int beginIndex = 0, endIndex = 0;
        auto it = nodes.begin();
        while (*it != edge->begin) {
            ++beginIndex;
            ++it;
        }
        it = nodes.begin();
        while (*it != edge->end) {
            ++endIndex;
            ++it;
        }

        matrix[beginIndex][endIndex] = edge->weight;
        // Uncomment the line below if the graph is undirected
        // matrix[endIndex][beginIndex] = edge->weight;
    }

    return matrix;
}

std::list<Node*> GraphHelper::adjacencyList(Node* node) {
    std::list<Node*> adjacency;
    std::list<Edge*> edges = graph.getEdges();
    for (const auto& edge : edges) {
        if (edge->begin == node) {
            adjacency.push_back(edge->end);
            // Uncomment the line below if the graph is undirected
            // adjacency.push_back(edge->begin);
        }
    }
    return adjacency;
}

std::vector<std::vector<short>> GraphHelper::incidenceMatrix() {
    std::hash_set<Node*> nodes = graph.getNodes();
    std::list<Edge*> edges = graph.getEdges();
    std::vector<std::vector<short>> matrix(nodes.size(), std::vector<short>(edges.size(), 0));

    int edgeIndex = 0;
    for (const auto& edge : edges) {
        int beginIndex = 0, endIndex = 0;
        auto it = nodes.begin();
        while (*it != edge->begin) {
            ++beginIndex;
            ++it;
        }
        it = nodes.begin();
        while (*it != edge->end) {
            ++endIndex;
            ++it;
        }

        matrix[beginIndex][edgeIndex] = 1;
        // matrix[endIndex][edgeIndex] = 1; // Uncomment if the graph is undirected
        ++edgeIndex;
    }

    return matrix;
}

std::vector<std::vector<int>> GraphHelper::weightMatrix() {
    std::hash_set<Node*> nodes = graph.getNodes();
    int size = nodes.size();
    std::vector<std::vector<int>> matrix(size, std::vector<int>(size, 0));

    std::list<Edge*> edges = graph.getEdges();
    for (const auto& edge : edges) {
        int beginIndex = 0, endIndex = 0;
        auto it = nodes.begin();
        while (*it != edge->begin) {
            ++beginIndex;
            ++it;
        }
        it = nodes.begin();
        while (*it != edge->end) {
            ++endIndex;
            ++it;
        }

        matrix[beginIndex][endIndex] = edge->weight;
        // Uncomment the line below if the graph is undirected
        // matrix[endIndex][beginIndex] = edge->weight;
    }

    return matrix;
}

std::list<Edge*> GraphHelper::minSpanningTree() {
    std::list<Edge*> result;
    std::unordered_set<Node*> visitedNodes;
    std::priority_queue<std::pair<int, Edge*>, std::vector<std::pair<int, Edge*>>, std::greater<std::pair<int, Edge*>>> minHeap;

    std::hash_set<Node*> nodes = graph.getNodes();
    if (!nodes.empty()) {
        visitedNodes.insert(*nodes.begin());

        std::list<Edge*> edges = graph.getEdges();
        for (const auto& edge : edges) {
            if (edge->begin == *nodes.begin() || edge->end == *nodes.begin()) {
                minHeap.push({ edge->weight, edge });
            }
        }

        while (!minHeap.empty() && visitedNodes.size() < nodes.size()) {
            Edge* minEdge = minHeap.top().second;
            minHeap.pop();

            Node* nextNode = nullptr;
            if (visitedNodes.count(minEdge->begin) && !visitedNodes.count(minEdge->end)) {
                nextNode = minEdge->end;
            }
            else if (visitedNodes.count(minEdge->end) && !visitedNodes.count(minEdge->begin)) {
                nextNode = minEdge->begin;
            }

            if (nextNode != nullptr) {
                visitedNodes.insert(nextNode);
                result.push_back(minEdge);

                for (const auto& edge : edges) {
                    if ((edge->begin == nextNode && !visitedNodes.count(edge->end)) ||
                        (edge->end == nextNode && !visitedNodes.count(edge->begin))) {
                        minHeap.push({ edge->weight, edge });
                    }
                }
            }
        }
    }

    return result;
}
