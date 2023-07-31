using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Graph.MainWindow;

namespace Graph
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Graph graph;
        public MainWindow()
        {
            InitializeComponent();
            // Initialize your graph here (add vertices and edges)
            graph = new Graph();
            graph.AddVertex(1, 50, 50);
            graph.AddVertex(2, 200, 50);
            graph.AddVertex(3, 125, 150);
            graph.AddVertex(4, 200, 125);
            graph.AddVertex(5, 275, 75);
            graph.AddVertex(6, 275, 175);
            graph.AddVertex(7, 325, 125);
            graph.AddEdge(1, 2, 5);
            graph.AddEdge(2, 3, 7);
            graph.AddEdge(1, 3, 15);
            graph.AddEdge(3, 4, 3);
            graph.AddEdge(4, 5, 6);
            graph.AddEdge(4, 6, 3);
            graph.AddEdge(5, 7, 6);
            graph.AddEdge(6, 7, 4);

            // Draw the graph
            DrawGraph(graph);

            // Call DijkstraShortestPath function and display the result
            int startVertexId = 1;
            int endVertexId = 7;
            var shortestPath = DijkstraShortestPath(graph, startVertexId, endVertexId);

            // Display the shortest path in the TextBlock
            DisplayShortestPath(shortestPath);
        }
        public class Vertex
        {
            public int Id { get; }
            public double X { get; }
            public double Y { get; }

            public Vertex(int id, double x, double y)
            {
                Id = id;
                X = x;
                Y = y;
            }
        }
        public class Graph
        {
            public Dictionary<int, List<(Vertex neighbor, int weight)>> AdjacencyList { get; }

            public Graph()
            {
                AdjacencyList = new Dictionary<int, List<(Vertex, int)>>();
            }

            // Add vertices and edges as needed
            public void AddVertex(int vertexId, double x, double y)
            {
                if (!AdjacencyList.ContainsKey(vertexId))
                    AdjacencyList[vertexId] = new List<(Vertex, int)>();

                AdjacencyList[vertexId].Add((new Vertex(vertexId, x, y), 0));
            }

            public void AddEdge(int sourceId, int destinationId, int weight)
            {
                //AddVertex(sourceId, 0, 0);
                //AddVertex(destinationId, 0, 0);
                // If the edge between source and destination already exists, do not add again (for undirected graphs)
                if (!AdjacencyList[sourceId].Any(v => v.Item1.Id == destinationId))
                {
                    AdjacencyList[sourceId].Add((AdjacencyList[destinationId][0].Item1, weight));
                    // Uncomment the line below for directed graphs (only add one direction)
                    // AdjacencyList[destinationId].Add((AdjacencyList[sourceId][0].Item1, weight));
                }
            }
        }
        private void DrawGraph(Graph graph)
        {
            // Clear previous drawings
            canvas.Children.Clear();

            // Draw vertices as ellipses or any other shape you prefer
            foreach (var vertexData in graph.AdjacencyList.Values)
            {
                Vertex vertex = vertexData[0].Item1;
                Ellipse ellipse = new Ellipse
                {
                    Width = 20,
                    Height = 20,
                    Fill = Brushes.Blue
                };

                // Set positions for vertices
                Canvas.SetLeft(ellipse, vertex.X);
                Canvas.SetTop(ellipse, vertex.Y);
                canvas.Children.Add(ellipse);

                // Draw edges and their weights
                foreach (var (neighbor, weight) in vertexData.Skip(1))
                {
                    Line line = new Line
                    {
                        X1 = vertex.X + 10, // Adjust offset for line start point
                        Y1 = vertex.Y + 10,
                        X2 = neighbor.X + 10, // Adjust offset for line end point
                        Y2 = neighbor.Y + 10,
                        Stroke = Brushes.Black,
                        StrokeThickness = 2
                    };

                    canvas.Children.Add(line);

                    TextBlock weightLabel = new TextBlock
                    {
                        Text = weight.ToString(),
                        Foreground = Brushes.Red
                    };

                    // Adjust offset for weight label position
                    Canvas.SetLeft(weightLabel, (vertex.X + neighbor.X) / 2);
                    Canvas.SetTop(weightLabel, (vertex.Y + neighbor.Y) / 2);
                    canvas.Children.Add(weightLabel);
                }
            }
        }

        public List<int> DijkstraShortestPath(Graph graph, int startVertexId, int endVertexId)
        {
            Dictionary<int, int> distance = new Dictionary<int, int>();
            Dictionary<int, int> previous = new Dictionary<int, int>();
            List<int> unvisitedVertices = new List<int>();

            foreach (var vertexData in graph.AdjacencyList.Values)
            {
                Vertex vertex = vertexData[0].Item1;
                distance[vertex.Id] = int.MaxValue;
                previous[vertex.Id] = -1;
                unvisitedVertices.Add(vertex.Id);
            }

            distance[startVertexId] = 0;

            int sum = 0;
            while (unvisitedVertices.Count > 0)
            {
                // Find the vertex with the minimum distance among unvisited vertices
                int currentVertexId = unvisitedVertices.OrderBy(v => distance[v]).First();
                unvisitedVertices.Remove(currentVertexId);

                if (distance[currentVertexId] == int.MaxValue)
                {
                    // All remaining vertices are inaccessible from the start vertex
                    break;
                }

                // Update distances and previous vertex for neighboring vertices
                foreach (var (neighbor, weight) in graph.AdjacencyList[currentVertexId].Skip(1))
                {
                    int altDistance = distance[currentVertexId] + weight;
                    if (altDistance < distance[neighbor.Id])
                    {
                        distance[neighbor.Id] = altDistance;
                        previous[neighbor.Id] = currentVertexId;
                        sum += weight;
                    }
                }
            }

            // Reconstruct the shortest path
            List<int> shortestPath = new List<int>();
            int vertexAt = endVertexId;
            while (vertexAt != -1)
            {
                shortestPath.Insert(0, vertexAt);
                vertexAt = previous[vertexAt];
            }
            totalLabel.Text = sum.ToString();
            return shortestPath;
        }
        private void DisplayShortestPath(List<int> shortestPath)
        {
            if (shortestPath.Count == 0)
            {
                shortestPathTextBlock.Text = "No path found.";
                return;
            }

            StringBuilder pathBuilder = new StringBuilder();
            foreach (int vertexId in shortestPath)
            {
                pathBuilder.Append(vertexId).Append(" -> ");
            }
            //pathBuilder.Length -= 4; // Remove the last " -> "
            shortestPathTextBlock.Text = "Shortest path: " + pathBuilder.ToString();
        }
    }
}
