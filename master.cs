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
	
	public static int topMargin = 10;
	public static int midMargin = 30;
	public static int topSelChance = 100 - (topMargin + midMargin);
	public static int midSelChance = midMargin;
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


	Label timelabel; 
	Label generationlabel; 
	Godot.Timer clock;
	Label dist; 
	OptionButton dropdown; 
	Button circle; 
	Button renew; 
	SpinBox townsfield;
	SpinBox refreshRate;
	Stopwatch watch = new Stopwatch();
	int temp;
	int fit = 1000;
	int popsize = 200;
	float learningRate = 0;
	int towns = 5;
	int threshold;
	int offX = 625;
	int offY = 440;
	int r = 250;
	int displaymode = 1;
	int generation = 0;
	double totalCost = 0;
	int selected = 0;
	int timesRefreshed = 0;
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
		
		townsfield.Value = towns;
		threshold = factorial(towns);
		refreshRate.Value = learningRate;

		init();

	}

	async void Genetic(){
		int threadId = timesRefreshed;
		float breedersRatio = 0.2f;
		int breedersCount = Mathf.FloorToInt(popsize * breedersRatio);
		

		float mutationRate = 0.1f;

		List<Specimen> population = new List<Specimen>();

		for(int i = 0; i < popsize; i++){
			List<Int32> genes = populate();
			population.Add(new Specimen(Math.Round(1000/cost(genes), 8), genes));
		}

		population = population.OrderByDescending(x => x.fitness).ToList();
		// GD.Print(SelectionTable["top"] + " " + SelectionTable["mid"] + " " + SelectionTable["low"]);
		GD.Print("Best : " + population[0].fitness);
		GD.Print("Worst : " + population[popsize-1].fitness);
		GD.Print("");

		while(true){
			generation += 1;

			//Selection
			List<Specimen> breedersList = breedersSelector(population, breedersCount).OrderByDescending(x => x.fitness).ToList();
			foreach(Specimen s in breedersList){
				GD.Print("S : " + s.fitness);
			}
			GD.Print("");


			//Crossover


			breedersList.OrderByDescending(x => x.fitness).ToList();
			topSpecimen = breedersList[0];

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
			GD.Print("T : " + s.fitness);
		}

		List<Specimen> newPop = new List<Specimen>();

		return newPop;
	}


	List<Specimen> breedersSelector(List<Specimen> previousGeneration, int breedersCount){
		List<Specimen> breedersList = new List<Specimen>();
		int fragment = previousGeneration.Count()/breedersCount;
		var rng = new Random();
		int top = popsize * Margin.topMargin / 100;
		int mid = popsize * Margin.midMargin / 100;

		for(int i = 0; i < breedersCount; i++){
			int rand = rng.Next(1, 100);
			int indexToAdd = rand < Margin.topSelChance ? rng.Next(0, top)
							: (rand >= Margin.topSelChance && rand < 100 - Margin.topMargin ? rng.Next(top, mid)
							: rng.Next(mid, popsize));

			if(!containsSpecimen(breedersList, previousGeneration[indexToAdd])){
				breedersList.Add(previousGeneration[indexToAdd]);
			}
			else if(breedersList.Count < threshold){
				i -= 1;
			}
		}

		if(!containsSpecimen(breedersList, previousGeneration[0])){
			breedersList[0] = previousGeneration[0];
		}


		return breedersList;
	}

	int factorial(int x){
		if(x == 0) return x;
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
			if(specimen.fitness == s.fitness) return true;
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
			GenerateRangeInCircle(Squares);
		}
		else{
			for(int i = 0; i < towns; i++){
				Cities.Add(GenerateRange(Squares));
			}
		}
		
		List<Int32> genes = populate();
		topSpecimen = new Specimen(Math.Round(1000/cost(genes), 8), genes);

		clock.WaitTime = Convert.ToSingle(1/refreshRate.Value);
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

	void GenerateRangeInCircle(List<Sqr> Squares){
		double angle = 360 / towns;
		int x1, y1;

		for(int i = 0; i < towns; i++){
			x1 = Convert.ToInt32(r * Math.Cos(i * 2 * Math.PI / towns)) + offX;
			y1 = Convert.ToInt32(r * Math.Sin(i * 2 * Math.PI / towns)) + offY;
			Cities.Add(new Vector2(x1, y1));
		}
	}


	Vector2 GenerateRange(List<Sqr> Squares){
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







