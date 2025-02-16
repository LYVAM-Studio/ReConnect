using System;
using System.Collections.Generic;
using Reconnect.Electronics.Components;
using Reconnect.Electronics.Graph;
/*
Vertice In = new Vertice("In",new CircuitInput());
Vertice Out = new Vertice("Out",new CircuitOutput());

Graph g = new Graph("test", In, Out);
Vertice r1 = new Vertice("r1",new IElecComponent());
Vertice r2 = new Vertice("r2",new IElecComponent());
Vertice r3 = new Vertice("r3",new IElecComponent());
Vertice l1 = new Vertice("l1",new IElecComponent());
Vertice l2 = new Vertice("l2",new IElecComponent());
Vertice s = new Vertice("s",new IElecComponent());

Vertice n1 = new Vertice("n1", new Node());
Vertice n2 = new Vertice("n2", new Node());

List<Vertice> verticesList = new List<Vertice> {r1, r1, r3, l1, l2, s, n1, n2};
In.AddAdjacent(new List<Vertice>{r1, l1, r2});
Out.AddAdjacent(l2);
n1.AddAdjacent(new List<Vertice> {l1, r2, s});
n2.AddAdjacent(new List<Vertice> {r3, s, l2});

r1.AddAdjacent(new List<Vertice> {In, r3});
r2.AddAdjacent(new List<Vertice> {In, n1});
r3.AddAdjacent(new List<Vertice> {r1, n2});
l1.AddAdjacent(new List<Vertice> {In, n1});
l2.AddAdjacent(new List<Vertice> {n2, Out});
s.AddAdjacent(new List<Vertice> {n1, n2});
g.AddVertice(verticesList);
g.DefineBranches();
Console.WriteLine('\n');
foreach (Branch branch in g.Branches)
{
    Console.WriteLine(branch);
}
*/