using Godot;
using System;
using io = System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using C = System.Globalization.CultureInfo;
using T = System.Threading.Thread;


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

	Label timelabel, generationlabel, dist, fitnesslabel;
	SpinBox vertexfield, refreshRate, breedSpinbox, popsizeSpinbox, mutationSpinbox, batchField, maxIterField, sliceField;
	Godot.Timer clock;
	OptionButton dropdown, optimizeField;
	Button circle, renew, stop, breedLabel, popsizeLabel, mutationLabel, launchBatch;
	Stopwatch watch = new Stopwatch();
	float[,,] results;
	bool batchMode = false;
	bool ongoing = false;
	int algos;
	int maxIter = 10;
	int batchSize = 1;
	int batchStack = 0;
	int slice = 10;
	int fit = 10000;
	int popsize = 200;
	float learningRate = 0;
	int vertex = 5;
	int threshold;
	int offX = 625;
	int offY = 440;
	int radius = 250;
	int displaymode = 1;
	int generation = 0;
	int timesRefreshed = 0;
	float mutationRate = 0.3f;
	float breedersRatio = 0.2f;
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
		Update();

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
		T.CurrentThread.CurrentCulture = C.GetCultureInfo("en-US");
		timelabel = GetNode<Label>("Timelabel");
		fitnesslabel = GetNode<Label>("FitnessLabel");
		generationlabel = GetNode<Label>("Generationlabel");
		clock = GetNode<Godot.Timer>("Clock");
		dist = GetNode<Label>("Dist");
		dropdown = GetNode<OptionButton>("Dropdown");
		optimizeField = GetNode<OptionButton>("OptimizeField");
		circle = GetNode<Button>("Circle");
		renew = GetNode<Button>("Renew");
		stop = GetNode<Button>("Stop");
		vertexfield = GetNode<SpinBox>("VertexField");
		sliceField = GetNode<SpinBox>("SliceField");
		batchField = GetNode<SpinBox>("BatchField");
		maxIterField = GetNode<SpinBox>("MaxIterField");
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
		dxionary.Add("Partially Mapped", partiallyMapped);
		dxionary.Add("Ordered", ordered);
		dxionary.Add("Alternating Edges", alternatingEdges);
		dxionary.Add("Cycle", cycle);
		//dxionary.Add("Sequential Constructive", sequentialConstructive);
		//dxionary.Add("BCSCX", bidirectionalCircularSequentialConstructive);
		//dxionary.Add("ASCX", adaptativeSequentialConstructive);

		dropdown.AddItem("Greedy", 0);
		dropdown.AddItem("Partially Mapped", 1);
		dropdown.AddItem("Ordered", 2);
		dropdown.AddItem("Alternating Edges", 3);
		dropdown.AddItem("Cycle", 4);
		//dropdown.AddItem("Sequential Constructive", 5);
		// dropdown.AddItem("BCSCX", 6);
		// dropdown.AddItem("ASCX", 7);

		dropdown.Selected = 4;

		optimizeField.AddItem("Yes", 0);
		optimizeField.AddItem("No", 1);

		optimizeField.Selected = 0;

		algos = dxionary.Count;
		mutationSpinbox.Value = mutationRate*10;
		breedSpinbox.Value = breedersRatio*10;
		popsizeSpinbox.Value = popsize;
		maxIterField.Value = maxIter;
		batchField.Value = batchSize;
		sliceField.Value = slice;
		
		vertexfield.Value = vertex;
		refreshRate.Value = learningRate;

		init();

	}

	void init(){
		generateSimulation();
		GD.Print("");

		if(!batchMode){
		GD.Print("Commencing");
			Genetic();
		}
		else{
			GD.Print("Commencing batch");
			batchStack = algos * batchSize - 1;
			results = new float[algos, batchSize, maxIter/slice+1];
		}
	}

	void generateSimulation(){

		vertex = Convert.ToInt32(vertexfield.Value);
		learningRate = Convert.ToInt32(refreshRate.Value);
		breedersRatio = Convert.ToSingle(breedSpinbox.Value/10);
		mutationRate = Convert.ToSingle(mutationSpinbox.Value/10);
		popsize = Convert.ToInt32(popsizeSpinbox.Value);
		maxIter = Convert.ToInt32(maxIterField.Value);
		batchSize = Convert.ToInt32(batchField.Value);
		slice = Convert.ToInt32(sliceField.Value);

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
		topSpecimen = new Specimen(Math.Round(fit/cost(genes), 8), genes);

		clock.WaitTime = Convert.ToSingle(1/learningRate);
		threshold = vertex <= 10 ? factorial(vertex) : 100000;
		watch.Restart();
		clock.Start();
	}

	void destroy(){
		Cities.Clear();
		topSpecimen = null;
		timesRefreshed += 1;
		
		clock.Stop();
	}

	async void Genetic(){
		int breedersCount = Mathf.FloorToInt(popsize * breedersRatio);
		int threadId = timesRefreshed;
		ongoing = batchMode ? true : false;
		generation = 0;
		addResult(topSpecimen.fitness);

		List<Specimen> population = new List<Specimen>();
		Random r = new Random();

		for(int i = 0; i < popsize; i++){
			List<Int32> genes = populate();
			population.Add(new Specimen(Math.Round(fit/cost(genes), 8), genes));
		}

		population = population.OrderByDescending(x => x.fitness).ToList();
		// GD.Print("");
		// GD.Print("Best : " + population[0].fitness);
		// GD.Print("Worst : " + population[popsize-1].fitness);
		// GD.Print("");

		while(true){
			if(threadId != timesRefreshed){
				break;
			}

			generation++;
			int top = popsize * Margin.topProbability / 100;
			int mid = popsize * Margin.midProbability / 100;
			
			//Selection
			List<Specimen> breedersList = breedersSelector(population, breedersCount).OrderByDescending(x => x.fitness).ToList();

			//Crossover
			population = Crossover(breedersList).OrderByDescending(x => x.fitness).ToList(); 

			
			topSpecimen = population[0];
			Update();

			//Mutation
			for(int i = 1; i < popsize; i++){
				float rand = Convert.ToSingle(r.Next(0, 100))/100;
				if(rand < mutationRate){
					swap(population[i].genes, r.Next(0, vertex-1), r.Next(0, vertex-1));
				}
			}

			addResult(topSpecimen.fitness);

			if(maxIter > 0 && generation >= maxIter){
				ongoing = false;
				watch.Stop();
				//writeResults();
				break;
			}
			if(learningRate == 0){
				watch.Stop();
			}
			
			await ToSignal(clock, "timeout");
		}
	}

	void addResult(double fitness){
		if(!batchMode || !(generation % slice == 0)) return;

		int row = dropdown.Selected;
		int column = batchStack%batchSize < 0 ? batchSize - 1 : batchStack%batchSize;	//OOH OOH MONKEY BANANA
		int depth = generation == 0 ? 0 : (generation-(generation%slice))/slice;
		// GD.Print("Adding - " + Math.Round(fitness, 3) + " - " + " x [" + row + "] y [" + column + "] z [" + depth + "]" + " G " + generation + " S " + batchStack + " B " + batchSize);
		results[row, column, depth] = Convert.ToSingle(fitness);
	}

	List<Specimen> Crossover(List<Specimen> breedersList){
		if(optimizeField.Selected == 0){
			foreach(Specimen _ in breedersList){
				optimize(_.genes, fit, vertex, 0, 1);
				_.fitness = Math.Round(fit/cost(_.genes), 8);
			}
		}

		DXover xover = dxionary[dropdown.Text];

		Specimen[] newPop = new Specimen[popsize];
		newPop[0] = breedersList[0];

		Parallel.For(1, popsize, _ =>{
			Random r = new Random();

			int idx1, idx2;
			idx1 = r.Next(0, breedersList.Count-1);
			do{
				idx2 = r.Next(0, breedersList.Count-1);
			}while(idx1 == idx2);

			List<int> individual = xover(breedersList[idx1].genes, breedersList[idx2].genes, vertex);
			newPop[_] = new Specimen(Math.Round(fit/cost(individual), 8), individual);
		});
		
		return newPop.ToList();
	}
	
	void optimize(List<Int32> lis, int fitness, int vertices, int start, int step){
		double previousFit = fitness/cost(lis);

		for(int i = start, j = start; j < vertices/step; i+=step, i%=vertices-1, j++){
			swap(lis, i, i+1);
			if(fitness/cost(lis) < previousFit){
				swap(lis, i, i+1);
				continue;
			}
			previousFit = fitness/cost(lis);
		}
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
		int[] childPath = new int[vertices];
		sanitize(childPath, -1);

		int idx = 0;
		do{
			childPath[idx] = s1[idx];
			idx = s1.FindIndex(x => x == s2[idx]);
		}
		while(idx != 0);
		
		for(int i = 0; i < vertices; i++){
			if(!childPath.Contains(s2[i])){
				childPath[i] = s2[i];
			}
		}
		
		return childPath.ToList();
	}

	//There was not that much documentation online for this one, so instead of selecting an edge at random where there is a conflict,
	//I added the rest of the parent to the offspring, which at least seems to produce viable results
	List<int> alternatingEdges(List<int> s1, List<int> s2, int vertices){

		Random r = new Random();
		List<int> childPath = new List<int>();
		int idx = r.Next(0, vertices-1);
		childPath.Add(s1[idx]);
		idx = (idx + 1) % vertices;
		childPath.Add(s1[idx]);
		String current = "s2";

		while(childPath.Count != vertices){
			List<int> parent = current == "s1" ? s1 : s2;
			List<int> previousList = current == "s1" ? s2 : s1;

			idx = parent.FindIndex(x => x == previousList[idx]);
			idx = (idx + 1) % vertices;

			if(!childPath.Contains(parent[idx])){
				childPath.Add(parent[idx]);
			}
			else{
				for(int i = 0; i < vertices; i++){
					if(!childPath.Contains(parent[i])){
						childPath.Add(parent[i]);
					}
				}
			}
			current = current == "s1" ? "s2" : "s1";
		}

		return childPath;
	}

	List<int> ordered(List<int> s1, List<int> s2, int vertices){

		Random r = new Random();
		int[] childPath = new int[vertices];

		int lower = r.Next(0, vertices-1);
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

		Random r = new Random();
		int[] childPath = new int[vertices];
		sanitize(childPath, -1);

		int lower = r.Next(0, vertices-1);
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

		Random r = new Random();
		List<int> childPath = new List<int>();
		int startIdx = r.Next(0, vertices-1);

		childPath.Add(s1[startIdx]);

		for(int i = 1; i < vertices; i++){
			int cur = childPath[i-1];

			int curIdx1 = s1.FindIndex(x => x == cur);
			int curIdx2 = s2.FindIndex(x => x == cur);

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
				int k = s1.FirstOrDefault(x => !childPath.Contains(x));
				childPath.Add(k);
			}
			else{
				childPath.Add(candidates[smallestIdx]);
			}
		}

		return childPath;
	}

	List<Specimen> breedersSelector(List<Specimen> previousGeneration, int breedersCount){
		List<Specimen> breedersList = new List<Specimen>();
		Random r = new Random();
		int topRange = Margin.topDecisionRange;
		int midRange = Margin.midDecisionRange;
		int top = popsize * Margin.topProbability / 100;
		int mid = popsize * Margin.midProbability / 100;
		int topAdded = 0;

		breedersCount = breedersCount < threshold ? breedersCount : threshold;
		breedersList.Add(previousGeneration[0]);

		for(int i = 0; i < breedersCount-1; i++){
			int rand = r.Next(1, 100);
			int indexToAdd;
			
			if(topAdded == top){
				midRange = topRange + midRange;
				topRange = 0;
			}

			if(rand <= topRange){
				indexToAdd = r.Next(0, top-1);
				topAdded++;
			}
			else if(rand > topRange && rand <= topRange + midRange){
				indexToAdd = r.Next(top, mid-1);
			}
			else{
				indexToAdd = r.Next(top + mid, popsize-1);
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

	void swap(List<Int32> lis, int idx1, int idx2){
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
		if(batchMode){
			if(batchStack >= 0){
				if(!ongoing){
					destroy();
					dropdown.Selected = (batchStack - (batchStack % batchSize)) / batchSize;
					generateSimulation();
					//GD.Print(batchStack + " " + dropdown.Selected);
					Genetic();
					batchStack--;
				}
			}
			else if(!ongoing){
				batchMode = false;
				// for(int x = 0; x < algos; x++){
				// 	GD.Print("Algo " + x);
				// 	for(int y = 0; y < batchSize; y++){
				// 		GD.Print("Batch " + y);
				// 		for(int z = 0; z < maxIter/slice+1; z++){
				// 			// GD.Print("Slice " + z);
				// 			GD.Print(Math.Round(results[x, y, z], 4) + " | " + x + y + z);
				// 		}
				// 	}
				// 	GD.Print("");
				// }
				writeResults();
				writeParameters();
			}
		}

		TimeSpan ts = watch.Elapsed;
		dist.Text = Math.Round(cost(topSpecimen.genes)).ToString() + "px";
		timelabel.Text = string.Format("{0}.{1}", ts.Seconds, Decimal.Floor(ts.Milliseconds/100));
		generationlabel.Text = generation.ToString();
		fitnesslabel.Text = Math.Round(topSpecimen.fitness, 4).ToString();
	}

	void writeResults(){
		string path = "out\\res.csv";

		try{
			if(io.File.Exists(path)){
				io.File.Delete(path);
			}

			string[] lines = new string[algos+1];
			string first = "0";

			for(int i = 1; i < maxIter/slice+1;i++){
				first += "," + i * slice;
			}
			lines[0] = first;

			for(int x = 0; x < algos; x++){
				string line = "";
				for(int z = 0; z < maxIter/slice+1; z++){
					line += "," + average(x, z);
				}
				line = line.Remove(0,1);
				lines[x+1] = line;
			}

			foreach(String s in lines){
				GD.Print(s);
			}
			
			io.File.AppendAllLines(path, lines);
		}
		catch(Exception e){
			GD.Print(e);
		}
	}

	void writeParameters(){
		string path = "out\\param.csv";
		try{
			if(io.File.Exists(path)){
				io.File.Delete(path);
			}
			
			string opti = optimizeField.Selected == 0 ? "Yes" : "No";
			string[] lines = new string[2];
			lines[0] = "vertices,popsize,mutation,breeders,batchsize,optimize,pathing";
			lines[1] = vertex + "," + popsize + "," + mutationRate + "," + breedersRatio + "," + batchSize + "," + opti + "," + circle.Text;
			
			io.File.AppendAllLines(path, lines);
		}
		catch(Exception e){
			GD.Print(e);
		}
	}

	float average(int x, int z){
		float res = 0;
		
		for(int y = 0; y < batchSize; y++){
			res += results[x, y, z];
		}
		res /= batchSize;

		return res;
	}

	void _on_Circle_pressed()
	{
		circle.Text = circle.Text == "Circle" ? "Shuffle" : "Circle";
	}

	void _on_LaunchBatch_pressed(){
		batchMode = true;
		destroy();
		init();
		Update();
	}

	void _on_Renew_pressed()
	{
		batchMode = false;
		batchStack = 0;
		destroy();
		init();
		Update();
	}

	void _on_Stop_pressed()
	{
		clock.Stop();
		watch.Stop();
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
		Random r = new Random();
		int c = lis.Count;
		int temp;

		for(int i = 0; i < c; i++){
			int idx = r.Next(0, c - 1);
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
			x1 = Convert.ToInt32(radius * Math.Cos(i * 2 * Math.PI / vertex)) + offX;
			y1 = Convert.ToInt32(radius * Math.Sin(i * 2 * Math.PI / vertex)) + offY;
			Cities.Add(new Vector2(x1, y1));
		}
	}


	Vector2 GenerateVertex(List<Sqr> Squares){
		Random r = new Random();
		int flx = 75;
		int ceix = 1303;
		int fly = 93;
		int ceiy = 831;
		int x, y;

		while(true){
			x = r.Next(flx, ceix);

			foreach(Sqr Square in Squares){
				int sx1 = Square.x1;
				int sx2 = Square.x2;

				if(x >= sx1 && x <= sx2){
					while(true){
						y = r.Next(fly, ceiy);

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