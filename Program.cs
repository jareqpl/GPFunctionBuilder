
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NCalc;

namespace GeneticProgrammingOptimizer
{
    public static class RandomExtensions
    {
        public static double NextDouble(this Random RandGenerator, double MinValue, double MaxValue)
        {
            return RandGenerator.NextDouble() * (MaxValue - MinValue) + MinValue;
        }
    }
    public class Population
    {
        public int PopulationNumber { get; set; } = 0;
        public List<Individual> Individuals = new List<Individual>();

        public static Population Create(int size, Func<Individual> individualProvider)
        {
            var individuals = new List<Individual>();
            for (int i = 0; i < size; i++)
            {
                individuals.Add(individualProvider());
            }
            return new Population { Individuals = individuals };
        }
    }

    public class TerminationCriteria
    {
        public bool IsSatisfied(Population pop)
        {
            return false;
        }
    }

    public static class Utils
    {
        public static int PopulationSize = 60;
        public static double MutationProbability = 0.05;
        public static double CrossoverProbability = 1;
        public static int TournamentSize = 10;
        public static bool Elitism = true;
        public static int MaxDepth = 15;
        public static int InitialDepth = 3;
        public static int DatasetSize = 50;
    }

    
    public class Program
    {
        public static Random rnd = new Random();
        static void Test(string[] args)
        {
            var ind1 = new Individual();
            var ind2 = new Individual();
            var index = BinaryTree<string>.ReindexTree(ref ind1.Genes);
            var index2 = BinaryTree<string>.ReindexTree(ref ind2.Genes);
            Console.WriteLine("individual 1");
            ind1.Genes.Print();
            Console.WriteLine("Individual 2");
            ind2.Genes.Print();

            BinaryTree<string>.SwapSubtrees(1, 1, ref ind1.Genes, ref ind2.Genes);
            Console.WriteLine("After swappin");
            ind1.Genes.Print();
            ind2.Genes.Print();

            Console.WriteLine("Mutation:");
            new Mutation().Perform(ref ind1);
            ind1.Genes.Print();
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Depth >");
            Utils.MaxDepth = int.Parse(Console.ReadLine());
            Console.WriteLine("DataseSize >");
            Utils.DatasetSize = int.Parse(Console.ReadLine());
            Console.WriteLine("Initial depth (will be +3) >");

            Utils.InitialDepth = int.Parse(Console.ReadLine());

            var resultsFile = DateTime.Now.Ticks + ".csv";


            //  Test(args);
            //   return;
            // inicjalizacja
            var pop = new Population();
            var terminationCriteria = new TerminationCriteria();
            do
            {
                pop.Individuals.Add(new Individual(Utils.InitialDepth));
            } while (Utils.PopulationSize > pop.Individuals.Count);

            var crossover = new Crossover();
            var selection = new TournamentSelection();
            var mutation = new Mutation();

            var data = new List<DataRow>();
            using (FileStream fs = new FileStream(@"INPUT_DATA_HERE.csv", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader stream = new StreamReader(fs))
            {
                stream.ReadLine();
                while (stream.EndOfStream == false)
                {
                    var row = stream.ReadLine().Split(';').Select(x => double.Parse(x.Replace('.', ','))).ToList();
                    var parametersAmount = 5;
                    data.Add(new DataRow()
                    {
                        Parameters = row.Take(parametersAmount).ToArray(), 
                        SatisfactionValue = (int)row[13],
                        failedTimes = (int)row[parametersAmount + 1],
                        threadsMin = (int)row[parametersAmount + 2],
                        threadsMax = (int)row[parametersAmount + 3],
                        threadsAvg = (int)row[parametersAmount + 4],
                        ramMax = (int)row[parametersAmount + 5],
                        processorAvg = (int)row[parametersAmount + 6],
                        processorMax = (int)row[parametersAmount + 7],
                    });
                }
            }
           data = data.Take(Utils.DatasetSize).ToList();
            File.AppendAllText(resultsFile,
    $"DatasetSize: {data.Count()}; PopSize: {Utils.PopulationSize}; Mutation: {Utils.MutationProbability}; Crossover: {Utils.CrossoverProbability}, TournamentSize: {Utils.TournamentSize}, Elitism: {Utils.Elitism.ToString()}, MaxDepth: {Utils.MaxDepth} \n"
    );
            File.AppendAllText(resultsFile, "Epoch;BestFitness;Time;TreeDepth;Function \n");
            var fitness = new Fitness(data);

            Stopwatch sw = new Stopwatch();
            while (terminationCriteria.IsSatisfied(pop) == false)
            {
                sw.Start();
                pop.Individuals.RemoveAll(x => BinaryTree<string>.MaxDepth(x.Genes) > Utils.MaxDepth);

                //fitness measure
                fitness.Evaluate(pop.Individuals);
                var freshPopulation = new List<Individual>();
                if (Utils.Elitism == true)
                {
                    freshPopulation.Add(pop.Individuals.OrderByDescending(f => f.Fitness).First());
                }
                while (freshPopulation.Count < Utils.PopulationSize)
                {
                    var parent1 = selection.Select(pop.Individuals);

                    var parent2 = selection.Select(pop.Individuals.Where(i => BinaryTree<string>.ToExpression(i.Genes) != BinaryTree<string>.ToExpression(parent1.Genes)).ToList());
                    var children = crossover.Perform(parent1, parent2);

                    mutation.Perform(ref children[0]);
                    mutation.Perform(ref children[1]);
                    freshPopulation.Add(children[0]);
                    freshPopulation.Add(children[1]);
                }
                fitness.Evaluate(freshPopulation);
                pop.PopulationNumber++;
                pop.Individuals = freshPopulation;
                var best = pop.Individuals.OrderByDescending(f => f.Fitness).ToList();
                sw.Stop();


                File.AppendAllText(resultsFile, string.Format("{1};{0};{2};{3};{4}\n", best.First().Fitness, pop.PopulationNumber, sw.ElapsedMilliseconds,BinaryTree<string>.MaxDepth(best.First().Genes) ,best.First().ToString()));
                Console.WriteLine("Epoka: {1}, Trafnosc: {0}, Czas: {2} ms", best.First().Fitness, pop.PopulationNumber, sw.ElapsedMilliseconds);
            }
        }
    }

    public class DataRow
    {
        public int ramMax { get; set; }

        public double[] Parameters { get; set; }
        public int SatisfactionValue { get; set; }
        public int failedTimes { get; set; }
        public int threadsMin { get; set; }
        public int threadsMax { get; set; }
        public int threadsAvg { get; set; }
        public int processorAvg { get; set; }
        public int processorMax { get; set; }


        public DataRow()
        {
        }
    }

    public class Fitness
    {
        private readonly List<DataRow> _dataset;
        public Fitness(List<DataRow> dataset)
        {
            _dataset = dataset;
        }
        public double Function(Individual ind, DataRow dr)
        {
            var expression = BinaryTree<string>.ToExpression(ind.Genes);
            var ncalcExpr = new Expression(expression);
            var variables = "ABCDE";
            for(int i = 0; i < dr.Parameters.Length; i++)
            {
                ncalcExpr.Parameters[variables[i].ToString()] = dr.Parameters[i];
            }



            var result = ncalcExpr.Evaluate() as double?;

            if (result.HasValue)
            {

                return result.Value;
            }

            return Double.MinValue;

        }

        public void Evaluate(IEnumerable<Individual> pop)
        {
            var individuals = pop.ToList();
            foreach (Individual individual in individuals)
            {
                double fitness = 0;
                foreach (DataRow data in _dataset)
                {
                    var expectedResult = data.SatisfactionValue;
                    var result = Function(individual, data);
                    if (result <= double.MinValue)
                    {
                        fitness = double.MinValue;
                    }
                    else
                    {
                        fitness -= Math.Abs(result - expectedResult); // odejmujemy od fitness pomyłkę.
                    }
                }

                individual.Fitness = fitness; // fitness to suma pomylek. Jak 0 to jest najlepiej.
            }
        }
    }

    public class Mutation
    {
        public void Perform(ref Individual ind)
        {
            if (Program.rnd.NextDouble() > Utils.MutationProbability)
                return;

            var maxIndex = BinaryTree<string>.ReindexTree(ref ind.Genes);
            BinaryTree<string>.SwapTreeWithTarget(ref ind.Genes, Individual.GenerateSimpleTree(), Program.rnd.Next(0, maxIndex));
        }
    }

    public class Crossover
    {
        public Individual[] Perform(Individual p1, Individual p2)
        {
            if (Program.rnd.NextDouble() > Utils.CrossoverProbability)
                return new Individual[] { };

            var maxIndex1 = BinaryTree<string>.ReindexTree(ref p1.Genes);
            var maxIndex2 = BinaryTree<string>.ReindexTree(ref p2.Genes);
            var c1 = p1.Clone();
            var c2 = p2.Clone();
            BinaryTree<string>.SwapSubtrees(Program.rnd.Next(0, maxIndex1), Program.rnd.Next(0, maxIndex2), ref c1.Genes, ref c2.Genes);
            return new[] { c1, c2 };
        }
    }

    public class TournamentSelection
    {
        public Individual Select(IList<Individual> individuals)
        {
            Individual best = null;
            for (int i = 0; i < Utils.TournamentSize; i++)
            {
                var randomIndividual = individuals[Program.rnd.Next(0, individuals.Count - 1)];
                if (best == null || randomIndividual.Fitness > best.Fitness)
                {
                    best = randomIndividual;
                }
            }

            return best;
        }
    }
    public class RouletteWheelSelection
    {
        public int PopulationMinSize { get; set; }
        private static Random rnd = new Random();
        public void Perform(Population population)
        {
            var newIndividuals = new List<Individual>();
            var sum = population.Individuals.Sum(x => x.Fitness);
            double rand;
            do
            {
                rand = rnd.NextDouble(0, sum);
                double tempSum = 0;
                foreach (var individual in population.Individuals)
                {
                    tempSum += individual.Fitness;
                    if (tempSum > rand)
                    {
                        newIndividuals.Add(individual);
                        population.Individuals.Remove(individual);
                        break;
                    }

                }
            } while (population.Individuals.Count < PopulationMinSize);
        }
    }
}
