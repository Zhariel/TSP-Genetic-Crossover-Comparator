using Godot;
using System;
using System.Linq;
using System.Diagnostics;
//using T = System.Threading;
using System.Collections.Generic;


class Sqr{
	public int x1, y1, x2, y2;
	
	public Sqr(int x1, int y1, int x2, int y2){
		this.x1 = x1;
		this.y1 = y1;
		this.x2 = x2;
		this.y2 = y2;
	}
}

public static class Margin{
	
	public static int topDecisionRange = 60;
	public static int midDecisionRange = 30;
	public static int topProbability = 10;
	public static int midProbability = 30;
}

class Specimen{
	public double fitness;
	public List<Int32> genes;

	public Specimen(double fitness, List<Int32> genes){
		this.fitness = fitness;
		this.genes = genes;
	}
}

public class master : Node2D
{


	Label timelabel, generationlabel, dist;
	SpinBox vertexfield, refreshRate, breedSpinbox, popsizeSpinbox, mutationSpinbox;
	Godot.Timer clock;
	OptionButton dropdown;
	Button circle, renew, breedLabel, popsizeLabel, mutationLabel;
	Stopwatch watch = new Stopwatch();
	Random rng = new Random();
	int maxIter = 100000;
	int fit = 1000;
	int popsize = 200;
	float learningRate = 1;
	int vertex = 20;
	int threshold;
	int offX = 625;
	int offY = 440;
	int r = 250;
	int displaymode = 1;
	int generation = 0;
	int selected = 0;
	int timesRefreshed = 0;
	float mutationRate = 0.3f;
	float breedersRatio = 0.2f;
	double totalCost = 0;
	List<Sqr> Squares = new List<Sqr>();
	List<Vector2> Cities;
	Specimen topSpecimen;
	Color blue = new Color(0, 0, 1, 1);
	Color red = new Color(1, 0, 0, 1);
	Color black = new Color(0, 0, 0, 0.8f);
	Color white = new Color(1, 1, 1, 1);
	Color grey = new Color(0.1f, 0.1f, 0.1f, 1);
	float linewidth = 1.3f;

	public delegate List<int> DXover(List<int> s1, List<int> s2, int vertices);
	Dictionary<String, DXover> dxionary = new Dictionary<string, DXover>();


	public override void _Draw()
	{
		// foreach(Sqr Square in Squares){
		// 	DrawRect(new Rect2(Square.x1, Square.y1, Square.x2 - Square.x1, Square.y2 - Square.y1), new Color(1, 0, 0, 1), false, 2, false);
		// }

		for(int i = 0; i < vertex - 1; i++){
			DrawLine(Cities[topSpecimen.genes[i]], Cities[topSpecimen.genes[i+1]], grey, linewidth);
		}
			DrawLine(Cities[topSpecimen.genes[topSpecimen.genes.Count-1]], Cities[topSpecimen.genes[0]], grey, linewidth);
		foreach(Vector2 City in Cities){
			DrawCircle(City, 5, white);
			DrawCircle(City, 4, red);
		}
	}


	public override void _Ready()
	{
		timelabel = GetNode<Label>("Timelabel");
		generationlabel = GetNode<Label>("Generationlabel");
		clock = GetNode<Godot.Timer>("Clock");
		dist = GetNode<Label>("Dist");
		dropdown = GetNode<OptionButton>("Dropdown");
		circle = GetNode<Button>("Circle");
		renew = GetNode<Button>("Renew");
		vertexfield = GetNode<SpinBox>("VertexField");
		refreshRate = GetNode<SpinBox>("RefreshRate");
		popsizeSpinbox = GetNode<SpinBox>("PopsizeSpinbox");
		mutationSpinbox = GetNode<SpinBox>("MutationSpinbox");
		breedSpinbox = GetNode<SpinBox>("BreedSpinbox");
		clock.Connect("timeout", this, "on_timeout");
		circle.Text = displaymode == 0 ? "Circle" : "Shuffle";

		Squares.Add(new Sqr(75, 142, 780, 503));
		Squares.Add(new Sqr(135, 504, 780, 576));
		Squares.Add(new Sqr(283, 577, 780, 641));
		Squares.Add(new Sqr(459, 642, 780, 716));
		Squares.Add(new Sqr(575, 718, 692, 770));
		Squares.Add(new Sqr(607, 772, 649, 831));
		Squares.Add(new Sqr(782, 190, 880, 335));
		Squares.Add(new Sqr(924, 239, 990, 334));
		Squares.Add(new Sqr(782, 337, 1090, 700));
		Squares.Add(new Sqr(1071, 701, 1105, 773));
		//Squares.Add(new Sqr(1107, 748, 1129, 820));
		Squares.Add(new Sqr(1095, 264, 1193, 511));
		//Squares.Add(new Sqr(1174, 191, 1270, 263));
		//Squares.Add(new Sqr(1268, 93, 1289, 199));

		dxionary.Add("Greedy", greedy);
		dxionary.Add("PMX", partiallyMapped);
		dxionary.Add("OX", ordered);
		dxionary.Add("AEX", alternatingEdges);
		dxionary.Add("CX", cycle);
		//dxionary.Add("SCX", sequentialConstructive);
		//dxionary.Add("BCSCX", bidirectionalCircularSequentialConstructive);
		//dxionary.Add("ASCX", adaptativeSequentialConstructive);

		dropdown.AddItem("Greedy", 0);
		dropdown.AddItem("PMX", 1);
		dropdown.AddItem("OX", 2);
		dropdown.AddItem("AEX", 3);
		dropdown.AddItem("CX", 4);
		// dropdown.AddItem("SCX", 5);
		// dropdown.AddItem("BCSCX", 6);
		// dropdown.AddItem("ASCX", 7);

		dropdown.Selected = 3;

		mutationSpinbox.Value = mutationRate*10;
		breedSpinbox.Value = breedersRatio*10;
		popsizeSpinbox.Value = popsize;
		
		vertexfield.Value = vertex;
		refreshRate.Value = learningRate;

		init();

	}

	async void Genetic(){
		int threadId = timesRefreshed;
		int breedersCount = Mathf.FloorToInt(popsize * breedersRatio);

		List<Specimen> population = new List<Specimen>();

		for(int i = 0; i < popsize; i++){
			List<Int32> genes = populate();
			population.Add(new Specimen(Math.Round(1000/cost(genes), 8), genes));
		}

		population = population.OrderByDescending(x => x.fitness).ToList();
		// GD.Print(SelectionTable["top"] + " " + SelectionTable["mid"] + " " + SelectionTable["low"]);
		// GD.Print("");
		// GD.Print("Best : " + population[0].fitness);
		// GD.Print("Worst : " + population[popsize-1].fitness);
		// GD.Print("");

		while(true){
			generation += 1;
			int top = popsize * Margin.topProbability / 100;
			int mid = popsize * Margin.midProbability / 100;

			//Selection
			List<Specimen> breedersList = breedersSelector(population, breedersCount).OrderByDescending(x => x.fitness).ToList();

			//Crossover

			population = Crossover(breedersList).OrderByDescending(x => x.fitness).ToList(); 

			if(topSpecimen.fitness < population[0].fitness){
				topSpecimen = population[0];
				Update();
			}

			//Mutation
			for(int i = 1; i < popsize; i++){
				float rand = Convert.ToSingle(rng.Next(0, 100))/100;
				if(rand < mutationRate){
					swap(population[i].genes, rng.Next(0, vertex-1), rng.Next(0, vertex-1));
				}
			}


			if(generation == maxIter){
				return;
			}

			await ToSignal(clock, "timeout");
			if(threadId != timesRefreshed){
				break;
			}
		}
	}


	List<Specimen> Crossover(List<Specimen> breedersList){

		//Optimization
		foreach(Specimen s in breedersList){
			optimize(s.genes);
			s.fitness = Math.Round(1000/cost(s.genes), 8);
		}

		DXover xover = dxionary[dropdown.Text];

		List<Specimen> newPop = new List<Specimen>();
		newPop.Add(breedersList[0]);

		//T.Thread[] threads = new T.Thread[popsize-1];

		for(int i = 0; i < popsize-1; i++){
			int idx1, idx2;
			idx1 = rng.Next(0, breedersList.Count-1);
			do{
				idx2 = rng.Next(0, breedersList.Count-1);
			}while(idx1 == idx2);
			
			// threads[i] = new T.Thread(() =>{
				List<int> individual = xover(breedersList[idx1].genes, breedersList[idx2].genes, vertex);
				newPop.Add(new Specimen(fit/cost(individual), individual));
			// });
			// threads[i].Start();
		}
		
		// for(int i = 0; i < popsize-1; i++){
		// 	threads[i].Join();
		// }

		// if(newPop.Count < popsize){
		// 	GD.Print(newPop.Count);
		// }

		// //To fix the occasional mishap
		// while(newPop.Count < popsize){
		// 	int idx1, idx2;
		// 	idx1 = rng.Next(0, breedersList.Count-1);
		// 	do{
		// 		idx2 = rng.Next(0, breedersList.Count-1);
		// 	}while(idx1 == idx2);

		// 	List<int> individual = xover(breedersList[idx1].genes, breedersList[idx2].genes, vertex);
		// 	newPop.Add(new Specimen(fit/cost(individual), individual));
			
		// }
		
		return newPop;
	}

	List<int> adaptativeSequentialConstructive(List<int> s1, List<int> s2, int vertices){

		List<int> childPath = new List<int>();

		return childPath;
	}

	List<int> bidirectionalCircularSequentialConstructive(List<int> s1, List<int> s2, int vertices){

		List<int> childPath = new List<int>();

		return childPath;
	}

	List<int> sequentialConstructive(List<int> s1, List<int> s2, int vertices){

		List<int> childPath = new List<int>();

		return childPath;
	}

	List<int> cycle(List<int> s1, List<int> s2, int vertices){

		List<int> childPath = new List<int>();

		return childPath;
	}

	List<int> alternatingEdges(List<int> s1, List<int> s2, int vertices){

		List<int> childPath = new List<int>();
		int idx = rng.Next(0, vertices-1);
		childPath.Add(s1[idx]);
		idx = (idx + 1) % vertices;
		childPath.Add(s1[idx]);		
		String current = "s2";

		while(childPath.Count != vertices){
			List<int> currentList = current == "s1" ? s1 : s2;
			List<int> previousList = current == "s1" ? s2 : s1;

			idx = currentList.FindIndex(x => x == previousList[idx]);
			idx = (idx + 1) % vertices;

			if(!childPath.Contains(currentList[idx])){
				childPath.Add(currentList[idx]);
			}
			else{
				for(int i = 0; i < vertices; i++){
					if(!childPath.Contains(currentList[i])){
						childPath.Add(currentList[i]);
					}
				}
			}
			current = current == "s1" ? "s2" : "s1";
		}

		return childPath;
	}

	List<int> ordered(List<int> s1, List<int> s2, int vertices){
		int[] childPath = new int[vertices];

		int lower = rng.Next(0, vertices-1);
		int upper = (lower + (vertices / 2)) % vertices;

		for(int i = lower; i != upper; i++, i%=vertices){
			childPath[i] = s1[i];
		}

		for(int i = lower, j = lower, k = upper; j < lower+vertices; i++, i%=vertices, j++){
			int w = s2[i];
			if(!childPath.Contains(w)){
				childPath[k] = w;
				k++;
				k%=vertices;
			}
		}

		return childPath.ToList();
	}

	List<int> partiallyMapped(List<int> s1, List<int> s2, int vertices){
		int[] childPath = new int[vertices];
		sanitize(childPath, -1);

		int lower = rng.Next(0, vertices-1);
		int upper = (lower + (vertices / 2)) % vertices;

		for(int i = lower; i != upper; i++, i%=vertices){
			childPath[i] = s1[i];
		}

		for(int i = upper; i != lower; i++, i%=vertices){
			int j = s2[i];
			if(childPath.Contains(s2[i])){

				int idxToInsert;

				do{
					idxToInsert = s1.FindIndex(x => x == j);
					j = s2[idxToInsert];
				}while(childPath.Contains(j));
			}
			childPath[i] = j;
		}
		
		return childPath.ToList();
	}

	List<int> greedy(List<int> s1, List<int> s2, int vertices){

		List<int> childPath = new List<int>();
		int startIdx = rng.Next(0, vertices-1);

		childPath.Add(s1[startIdx]);

		for(int i = 1; i < vertices; i++){
			int curIdx1 = -1;
			int curIdx2 = -1;
			int cur = childPath[i-1];

			for(int j = 0; j < vertices; j++){
				if(s1[j] == cur){
					curIdx1 = j;
				}
			}
			for(int j = 0; j < vertices; j++){
				if(s2[j] == childPath[i-1]){
					curIdx2 = j;
				}
			}

			int next1 = curIdx1 != vertices-1 ? s1[curIdx1+1] : s1[0];
			int next2 = curIdx2 != vertices-1 ? s2[curIdx2+1] : s2[0];
			int prev1 = curIdx1 != 0 ? s1[curIdx1-1] : s1[vertices-1];
			int prev2 = curIdx2 != 0 ? s2[curIdx2-1] : s2[vertices-1];


			List<int> candidates = new List<int>();

			if(!childPath.Contains(next1)) candidates.Add(next1);
			if(!childPath.Contains(next2)) candidates.Add(next2);
			if(!childPath.Contains(prev1)) candidates.Add(prev1);
			if(!childPath.Contains(prev2)) candidates.Add(prev2);

			int smallestIdx = 0;
			float smallestDist = 999999;

			for(int k = 0; k < candidates.Count; k++){
				float dist = pythag(Cities[candidates[k]], Cities[cur]);
				if(dist < smallestDist){
					smallestDist = dist;
					smallestIdx = k;
				}
			}

			if(candidates.Count == 0){
				for(int k = 0; k < vertices; k++){
					if(!childPath.Contains(s1[k])){
						childPath.Add(s1[k]);
					}
				}
			}
			else{
				childPath.Add(candidates[smallestIdx]);
			}
		}

		return childPath;
	}

	List<Specimen> breedersSelector(List<Specimen> previousGeneration, int breedersCount){
		List<Specimen> breedersList = new List<Specimen>();
		int topRange = Margin.topDecisionRange;
		int midRange = Margin.midDecisionRange;
		int top = popsize * Margin.topProbability / 100;
		int mid = popsize * Margin.midProbability / 100;
		int topAdded = 0;

		breedersCount = breedersCount < threshold ? breedersCount : threshold;

		for(int i = 0; i < breedersCount; i++){
			int rand = rng.Next(1, 100);
			int indexToAdd;
			
			if(topAdded == top){
				midRange = topRange + midRange;
				topRange = 0;
			}

			if(rand <= topRange){
				indexToAdd = rng.Next(0, top-1);
				topAdded++;
			}
			else if(rand > topRange && rand <= topRange + midRange){
				indexToAdd = rng.Next(top, mid-1);
			}
			else{
				indexToAdd = rng.Next(top + mid, popsize-1);
			}

			if(!containsSpecimen(breedersList, previousGeneration[indexToAdd])){
				breedersList.Add(previousGeneration[indexToAdd]);
			}
			else{
				int idx = indexToAdd;
				for(int k = indexToAdd; k >= 0; k--){
					if(!containsSpecimen(breedersList, previousGeneration[k])){
						idx = k;
						break;
					}
				}
				if(idx == -1){
					for(int p = indexToAdd; p < popsize-1; p++){
						if(!containsSpecimen(breedersList, previousGeneration[p])){
							idx = p;
							break;
						}
					}
				}
							
				breedersList.Add(previousGeneration[idx]);
			}
		}
		if(!containsSpecimen(breedersList, previousGeneration[0])){	
			breedersList[0] = previousGeneration[0];
		}


		return breedersList;
	}

	public static void sanitize(int[] array, int val){
		for(int i = 0; i < array.Count(); i++){
			array[i] = val;
		}
	}

	int factorial(int x){
		if(x == 1) return x;
		else return x*factorial(x-1);
	}

	void optimize(List<Int32> lis){
		double originalCost = fit/cost(lis);

		for(int i = 0; i < lis.Count-2; i++){
			swap(lis, i, i+1);
			if(fit/cost(lis) < originalCost){
				swap(lis, i, i+1);
			}
		}
		swap(lis, 0, lis.Count-1);
		if(fit/cost(lis) < originalCost){
			swap(lis, 0, lis.Count-1);
		}
	}

	void swap(List<Int32> lis,int idx1, int idx2){
		int temp = lis[idx1];
		lis[idx1] = lis[idx2];
		lis[idx2] = temp;
	}

	Boolean containsSpecimen(List<Specimen> list, Specimen specimen){
		foreach(Specimen s in list){
			if(specimen.fitness == s.fitness){ return true;}
		}
		return false;
	}


	public override void _Process(float delta)
	{
		TimeSpan ts = watch.Elapsed;
		dist.Text = Math.Round(cost(topSpecimen.genes)).ToString() + "px";
		timelabel.Text = string.Format("{0}.{1}", ts.Seconds, Decimal.Floor(ts.Milliseconds/100));
		generationlabel.Text = generation.ToString();
		
	}

	void init(){
		Cities = new List<Vector2>();

		if(circle.Text == "Circle"){
			GenerateVertexInCircle(Squares);
		}
		else{
			for(int i = 0; i < vertex; i++){
				Cities.Add(GenerateVertex(Squares));
			}
		}
		
		List<Int32> genes = populate();
		topSpecimen = new Specimen(Math.Round(1000/cost(genes), 8), genes);

		clock.WaitTime = Convert.ToSingle(1/refreshRate.Value);
		threshold = vertex <= 10 ? factorial(vertex) : 100000;
		watch.Restart();
		clock.Start();

		GD.Print("");
		GD.Print("Commencing");

		Genetic();

	}

	void destroy(){
		Cities.Clear();
		topSpecimen = null;
		timesRefreshed += 1;
		generation = 0;
		
		clock.Stop();
	}

	private void _on_Circle_pressed()
	{
		circle.Text = circle.Text == "Circle" ? "Shuffle" : "Circle";
	}

	void _on_Renew_pressed()
	{
		destroy();
		vertex = Convert.ToInt32(vertexfield.Value);
		breedersRatio = Convert.ToSingle(breedSpinbox.Value/10);
		mutationRate = Convert.ToSingle(mutationSpinbox.Value/10);
		popsize = Convert.ToInt32(popsizeSpinbox.Value);
		init();
		Update();
	}

	List<Int32> populate(){
		List<Int32> lis = new List<Int32>();
		for(int i = 0; i < vertex; i++){
			lis.Add(i);
		}

		randomize(lis);
		return lis;
	}

	void randomize(List<Int32> lis){
		int c = lis.Count;
		int temp;

		for(int i = 0; i < c; i++){
			int idx = rng.Next(0, c - 1);
			temp = lis[i];
			lis[i] = lis[idx];
			lis[idx] = temp;
		}
	}

	float pythag(Vector2 w, Vector2 q){
		return Convert.ToSingle(Math.Sqrt(
			Math.Pow(Math.Abs(Convert.ToDouble(w.x) - Convert.ToDouble(q.x)), Convert.ToDouble(2)) + 
			Math.Pow(Math.Abs(Convert.ToDouble(w.y) - Convert.ToDouble(q.y)), Convert.ToDouble(2))));
	}


	double cost(List<Int32> lis){
		double cost = 0;
		for(int i = 0; i < lis.Count - 1; i++){
			cost += pythag(Cities[lis[i]], Cities[lis[i+1]]);
		}
			cost += pythag(Cities[lis[0]], Cities[lis[vertex-1]]);
			
		return cost;
	}

	void GenerateVertexInCircle(List<Sqr> Squares){
		double angle = 360 / vertex;
		int x1, y1;

		for(int i = 0; i < vertex; i++){
			x1 = Convert.ToInt32(r * Math.Cos(i * 2 * Math.PI / vertex)) + offX;
			y1 = Convert.ToInt32(r * Math.Sin(i * 2 * Math.PI / vertex)) + offY;
			Cities.Add(new Vector2(x1, y1));
		}
	}


	Vector2 GenerateVertex(List<Sqr> Squares){
		int flx = 75;
		int ceix = 1303;
		int fly = 93;
		int ceiy = 831;
		int x, y;

		while(true){
			x = rng.Next(flx, ceix);

			foreach(Sqr Square in Squares){
				int sx1 = Square.x1;
				int sx2 = Square.x2;

				if(x >= sx1 && x <= sx2){
					while(true){
						y = rng.Next(fly, ceiy);

						foreach(Sqr Square2 in Squares){
							sx1 = Square2.x1;
							sx2 = Square2.x2;
							int sy1 = Square2.y1;
							int sy2 = Square2.y2;

							if(x >= sx1 && x <= sx2 && y >= sy1 && y <= sy2){
								return new Vector2(x, y);
							}
						}
					}
				}
			}
		}
	}
}







