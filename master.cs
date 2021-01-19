using Godot;
using System;
using System.Linq;
using System.Diagnostics;
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
	SpinBox townsfield, refreshRate, breedSpinbox, popsizeSpinbox, mutationSpinbox;
	Godot.Timer clock;
	OptionButton dropdown;
	Button circle, renew, breedLabel, popsizeLabel, mutationLabel;
	Stopwatch watch = new Stopwatch();
	int temp;
	int fit = 1000;
	int popsize = 200;
	float learningRate = 20;
	int towns = 30;
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

	public override void _Draw()
	{
//		foreach(Sqr Square in Squares){
//			DrawRect(new Rect2(Square.x1, Square.y1, Square.x2 - Square.x1, Square.y2 - Square.y1), new Color(1, 0, 0, 1), false, 2, false);
//		}

		for(int i = 0; i < towns - 1; i++){
			DrawLine(Cities[topSpecimen.genes[i]], Cities[topSpecimen.genes[i+1]], red);
		}
			DrawLine(Cities[topSpecimen.genes[topSpecimen.genes.Count-1]], Cities[topSpecimen.genes[0]], red);
		foreach(Vector2 City in Cities){
			DrawCircle(City, 4, blue);
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
		townsfield = GetNode<SpinBox>("Townsfield");
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
		Squares.Add(new Sqr(1107, 748, 1129, 820));
		Squares.Add(new Sqr(1095, 264, 1193, 511));
		Squares.Add(new Sqr(1174, 191, 1270, 263));
		Squares.Add(new Sqr(1268, 93, 1289, 199));

		dropdown.AddItem("Greedy", 0);
		dropdown.AddItem("Exemple", 1);
		dropdown.AddItem("Exemple", 2);

		mutationSpinbox.Value = mutationRate*100;
		breedSpinbox.Value = breedersRatio*100;
		popsizeSpinbox.Value = popsize;
		
		townsfield.Value = towns;
		refreshRate.Value = learningRate;

		init();

	}

	async void Genetic(){
		int threadId = timesRefreshed;
		int breedersCount = Mathf.FloorToInt(popsize * breedersRatio);
		var rng = new Random();		

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
					swap(population[i].genes, rng.Next(0, towns-1), rng.Next(0, towns-1));
				}
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

		List<Specimen> newPop = new List<Specimen>();
		newPop.Add(breedersList[0]);
		var rng = new Random();

		for(int i = 1; i < popsize; i++){
			int idx1, idx2;
			idx1 = rng.Next(0, breedersList.Count-1);
			do{
				idx2 = rng.Next(0, breedersList.Count-1);
			}while(idx1 == idx2);
			
			List<int> individual = greedy(breedersList[idx1].genes, breedersList[idx2].genes);

			newPop.Add(new Specimen(fit/cost(individual), individual));
		}

		return newPop;
	}

	List<int> greedy(List<int> s1, List<int> s2){

		List<int> childPath = new List<int>();
		var rng = new Random();
		int startIdx = rng.Next(0, towns-1);

		childPath.Add(s1[startIdx]);

		for(int i = 1; i < towns; i++){
			int curIdx1 = -1;
			int curIdx2 = -1;
			int cur = childPath[i-1];

			for(int j = 0; j < towns; j++){
				if(s1[j] == cur){
					curIdx1 = j;
				}
			}
			for(int j = 0; j < towns; j++){
				if(s2[j] == childPath[i-1]){
					curIdx2 = j;
				}
			}

			int next1 = curIdx1 != towns-1 ? s1[curIdx1+1] : s1[0];
			int next2 = curIdx2 != towns-1 ? s2[curIdx2+1] : s2[0];
			int prev1 = curIdx1 != 0 ? s1[curIdx1-1] : s1[towns-1];
			int prev2 = curIdx2 != 0 ? s2[curIdx2-1] : s2[towns-1];


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
				for(int k = 0; k < towns; k++){
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
		var rng = new Random();
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
				int yello = 0;
				int idx = indexToAdd;
				for(int k = indexToAdd; k >= 0; k--){
					if(!containsSpecimen(breedersList, previousGeneration[k])){
						yello++;
						idx = k;
						break;
					}
				}
				int hello = 0;
				if(idx == -1){
					for(int p = indexToAdd; p < popsize-1; p++){
						hello++;
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
			for(int i = 0; i < towns; i++){
				Cities.Add(GenerateVertex(Squares));
			}
		}
		
		List<Int32> genes = populate();
		topSpecimen = new Specimen(Math.Round(1000/cost(genes), 8), genes);

		clock.WaitTime = Convert.ToSingle(1/refreshRate.Value);
		threshold = towns <= 10 ? factorial(towns) : 100000;
		watch.Restart();
		clock.Start();

		GD.Print("");
		GD.Print("Commencing");

		Genetic();

	}

	void destroy(){
		for(int i = towns - 1; i >= 0; i--){
			Cities.Remove(Cities[i]);
			//Path.Remove(Path[i]);
		}
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
		towns = Convert.ToInt32(townsfield.Value);
		init();
		Update();
	}

	List<Int32> populate(){
		List<Int32> lis = new List<Int32>();
		for(int i = 0; i < towns; i++){
			lis.Add(i);
		}

		randomize(lis);
		return lis;
	}

	void randomize(List<Int32> lis){
		var rng = new Random();
		int c = lis.Count;

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
			cost += pythag(Cities[lis[0]], Cities[lis[towns-1]]);
			
		return cost;
	}

	void GenerateVertexInCircle(List<Sqr> Squares){
		double angle = 360 / towns;
		int x1, y1;

		for(int i = 0; i < towns; i++){
			x1 = Convert.ToInt32(r * Math.Cos(i * 2 * Math.PI / towns)) + offX;
			y1 = Convert.ToInt32(r * Math.Sin(i * 2 * Math.PI / towns)) + offY;
			Cities.Add(new Vector2(x1, y1));
		}
	}


	Vector2 GenerateVertex(List<Sqr> Squares){
		int flx = 75;
		int ceix = 1303;
		int fly = 93;
		int ceiy = 831;
		int x, y;
		var rng = new Random();

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






