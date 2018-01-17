using System;
using System.Collections.Generic;
using AutoMapper;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Running;
using EmitMapper;
using FlashMapper;
using Mapster;
using Nelibur.ObjectMapper;
using ObjectMapper.Framework;

namespace MappersBenchmark
{
	class Program
	{
		static void Main(string[] args)
		{
			ConfigureMappings();

			var summary = BenchmarkRunner.Run<Adapter>();

			Console.ReadKey();
		}

		public static void ConfigureMappings()
		{
			Mapper.Initialize(cfg =>
			{
				cfg.CreateMap<ClassB, ClassA>();
			});

			TinyMapper.Bind<ClassB, ClassA>();


			ExpressMapper.Mapper.Register<ClassB, ClassA>();
			ExpressMapper.Mapper.Compile();


			var singleInit = FlashMapperConfig;
		}

		private static IMappingConfiguration mappingConfiguration;

		public static IMappingConfiguration FlashMapperConfig
		{
			get
			{
				if (mappingConfiguration == null)
				{
					mappingConfiguration = new MappingConfiguration();
					mappingConfiguration.CreateMapping<ClassB, ClassA>();
				}

				return mappingConfiguration;
			}
		}
	}

	public class NestedClass
	{
		public int NestedPropInt { get; set; }

		public string NestedPropString { get; set; }
	}

	public class ClassA
	{
		public int? Prop1 { get; set; }
		public string Prop2 { get; set; }
		public decimal Prop3 { get; set; }
		public List<int> Prop4 { get; set; }

		public NestedClass Prop5 { get; set; }

		public List<NestedClass> ClassArray { get; set; }
	}

	public class ClassB
	{
		public int? Prop1 { get; set; }
		public string Prop2 { get; set; }
		public decimal Prop3 { get; set; }
		public List<int> Prop4 { get; set; }

		public NestedClass Prop5 { get; set; }

		public List<NestedClass> ClassArray { get; set; }

		public Version DoNotMap { get; set; }
	}

	[RankColumn]
	public class Adapter : IObjectMapperAdapter<ClassA, ClassB>
	{
		public Adapter()
		{
			Program.ConfigureMappings();
		}

		public static readonly ClassB TargetTest = new ClassB
		{
			Prop1 = 777,
			Prop2 = "this",
			Prop3 = 34.45m,
			Prop4 = new List<int> { 1, 2, 777, 256 },
			Prop5 = new NestedClass { NestedPropInt = 1, NestedPropString = "NestedPropString" },
			ClassArray = new List<NestedClass> { new NestedClass { NestedPropInt = 17, NestedPropString = "string" },
												 new NestedClass { NestedPropInt = 89, NestedPropString = "nameof" }
											   }
		};

		public static ClassA MapByHandle(ClassB target)
		{
			var thi = new ClassA();
			thi.Prop1 = target.Prop1;
			thi.Prop2 = target.Prop2;
			thi.Prop3 = target.Prop3;


			thi.Prop4 = new List<int>();
			for (int i = 0; i < target.Prop4.Count; i++)
				thi.Prop4.Add(target.Prop4[i]);

			thi.Prop5 = new NestedClass { NestedPropInt = target.Prop5.NestedPropInt, NestedPropString = target.Prop5.NestedPropString };


			thi.ClassArray = new List<NestedClass>();
			for (int i = 0; i < target.ClassArray.Count; i++)
			{
				var cl = new NestedClass();
				cl.NestedPropInt = target.ClassArray[i].NestedPropInt;
				cl.NestedPropString = target.ClassArray[i].NestedPropString;
				thi.ClassArray.Add(cl);
			}

			return thi;
		}

		public static ClassA MapObjectEmit(ClassB ClassB)
		{
			return ObjectMapperManager.DefaultInstance.GetMapper<ClassB, ClassA>().Map(TargetTest);
		}

		public static ClassA AutoMapper(ClassB ClassB)
		{
			return Mapper.Instance.Map<ClassB, ClassA>(ClassB);
		}

		public static ClassA TinyMapperMap(ClassB classB)
		{
			return TinyMapper.Map<ClassA>(classB);
		}

		public static ClassA ExpressMapperMap(ClassB classB)
		{
			return ExpressMapper.Mapper.Map<ClassB, ClassA>(classB);
		}

		public static ClassA FastMapperMap(ClassB classB)
		{
			return FastMapper.TypeAdapter.Adapt<ClassB, ClassA>(classB);
		}

		public static ClassA MapsterMap(ClassB classB)
		{
			return classB.Adapt<ClassA>();
		}

		public static ClassA AgileMapperMap(ClassB classB)
		{
			return AgileObjects.AgileMapper.Mapper.Map(classB).ToANew<ClassA>();
		}

		public static ClassA FlashMapperMap(ClassB classB)
		{
			return Program.FlashMapperConfig.Convert<ClassB>(classB).To<ClassA>();
		}

		[Benchmark(Description = "TinyMapper")]
		public ClassA TinyMapperTest() => TinyMapperMap(TargetTest);

		[Benchmark(Baseline = true)]
		public ClassA Handwritten() => MapByHandle(TargetTest);

		[Benchmark]
		public ClassA EmitMapper() => MapObjectEmit(TargetTest);

		[Benchmark]
		public ClassA AutoMapper() => AutoMapper(TargetTest);


		[Benchmark(Description = "ExpressMapper")]
		public ClassA ExpressMapperTest() => ExpressMapperMap(TargetTest);

		[Benchmark(Description = "FastMapper")]
		public ClassA FastMapperTest() => FastMapperMap(TargetTest);

		[Benchmark]
		public ClassA Mapster() => MapsterMap(TargetTest);

		[Benchmark]
		public ClassA AgileMapper() => AgileMapperMap(TargetTest);

		[Benchmark]
		public ClassA FlashMapper() => FlashMapperMap(TargetTest);

		public void MapObject(ClassA source, ClassB target)
		{
			target.Prop1 = source.Prop1 ?? default(int);
			target.Prop2 = source.Prop2;
			target.Prop3 = source.Prop3;
			target.Prop4.CopyFrom(source.Prop4);
		}

		public void MapObject(ClassB source, ClassA target)
		{
			target.Prop1 = source.Prop1;
			target.Prop2 = source.Prop2;
			target.Prop3 = source.Prop3;
			target.Prop4.CopyFrom(source.Prop4);
		}
	}
}
