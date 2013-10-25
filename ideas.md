## Optimizing repeated singleton access per resolution root:

* Current expression:

	(currentScope, resolutionScope) => 
		new Complex(
			value(IocPerformance.Classes.Complex.FirstService), 
			value(IocPerformance.Classes.Complex.SecondService), 
			value(IocPerformance.Classes.Complex.ThirdService), 
			new SubObjectOne(value(IocPerformance.Classes.Complex.FirstService)), 
			new SubObjectTwo(value(IocPerformance.Classes.Complex.SecondService)), 
			new SubObjectThree(value(IocPerformance.Classes.Complex.ThirdService)))

* Possible optimization - brings 3 less fields accesses. Replace them with local variable access:

	(currentScope, resolutionScope) => {
		var firstService = value(IocPerformance.Classes.Complex.FirstService); 
		var secondService = value(IocPerformance.Classes.Complex.SecondService);
		var thirdService = value(IocPerformance.Classes.Complex.ThirdService)
		new Complex(
			firstService, 
			secondService, 
			thirdService, 
			new SubObjectOne(firstService), 
			new SubObjectTwo(secondService), 
			new SubObjectThree(thirdService))
	}

* Another posibility:

	(currentScope, resolutionScope) => 
		new Func<X, Y, Z>((firstService, secondService, thirdService) =>		
			new Complex(
				firstService, 
				secondService, 
				thirdService, 
				new SubObjectOne(firstService), 
				new SubObjectTwo(secondService), 
				new SubObjectThree(thirdService)))
		.Invoke(
			value(IocPerformance.Classes.Complex.FirstService), 
			value(IocPerformance.Classes.Complex.SecondService), 
			value(IocPerformance.Classes.Complex.ThirdService))
