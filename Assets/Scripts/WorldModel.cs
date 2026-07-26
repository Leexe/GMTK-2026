using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class World
{
	public List<Floor> Floors;

	public void Generate(LevelSO genInfo, PersonGenInfoSO personGenInfo, RolesSO roleGenInfo)
	{
		Floors = new();

		for (int i = 0; i < genInfo.LevelsList.Count; i++)
		{
			Floors.Add(Floor.Generate(i + 1, genInfo.LevelsList[i], personGenInfo, roleGenInfo));
		}
	}
}

public class Floor
{
	public List<Person> People;

	public static Floor Generate(int floor, Level genInfo, PersonGenInfoSO personGenInfo, RolesSO roleGenInfo)
	{
		Floor retFloor = new() { People = new() };

		Dictionary<NpcRoles, int> spawnCounts = genInfo.GenerateSpawnCounts(roleGenInfo);

		var normalNpcRoles = new NpcRoles[] { NpcRoles.Worker, NpcRoles.Psychologist, NpcRoles.Guard };

		// add normal people
		foreach (NpcRoles role in normalNpcRoles)
		{
			for (int i = 0; i < spawnCounts[role]; i++)
			{
				retFloor.People.Add(Person.Create(role, floor, personGenInfo));
			}
		}

		// add skinwalkers
		for (int i = 0; i < spawnCounts[NpcRoles.Skinwalker]; i++)
		{
			NpcRoles role = normalNpcRoles[Random.Range(0, normalNpcRoles.Length)];
			retFloor.People.Add(Skinwalker.FromPerson(Person.Create(role, floor, personGenInfo)));
		}

		return retFloor;
	}
}

public class Person
{
	public virtual bool IsSkinwalker => false;

	public string Name;
	public NpcRoles Role;
	public int HeightInches;

	public List<int> AssignedFloors;
	public List<QnA> PossibleQNAResponses;

	//

	public static Person Create(NpcRoles role, int floor, PersonGenInfoSO genInfo)
	{
		return new()
		{
			Name = genInfo.RandomName(),
			Role = role,
			HeightInches = genInfo.RandomHeight(),

			AssignedFloors = new() { floor },
			PossibleQNAResponses = genInfo.RandomQnA(3, role),
		};
	}
}

public class Skinwalker : Person
{
	public override bool IsSkinwalker => true;
	public NpcRoles FakeRole = NpcRoles.Worker;
	public List<Person> Victims;

	//

	// TODO: a smarter/cooler skinwalker mangling system
	// - we should be deliberate about what gets mangled and what doesn't, for challenge
	// - smarter mangling, based on context abt floors and stuff
	public static Skinwalker FromPerson(Person person)
	{
		NpcRoles fakeRole = person.Role == NpcRoles.Skinwalker
			? (NpcRoles)Random.Range(0, 3)
			: person.Role;

		// skinwalkers are GUARANTEED to mess something up:
		// - height, with 25% chance
		// - assigned floor, with 25% chance
		// - qna, with 50% chance. note that a skinwalker may be undetectable if you don't have 4 psychologists.

		// - if not guaranteed, still a 20% chance per category.

		int messup = Random.Range(0, 4);
		bool mangleHeight = messup == 0 || Random.value < 0.2f;
		bool mangleFloors = messup == 1 || Random.value < 0.2f;
		bool mangleQNA = messup > 1 || Random.value < 0.2f;

		int mangledFloor = person.AssignedFloors[0];
		if (mangleFloors)
		{
			mangledFloor = ((mangledFloor + Random.Range(0, 9)) % 10) + 1;
		}

		// if mangleQNA, guarantee mangle first question in pool. other questions still have 20% mangle chance.
		List<QnA> mangledQNA = new()
		{
			mangleQNA ? person.PossibleQNAResponses[0].Mangled(0f, 0f) : person.PossibleQNAResponses[0].Mangled()
		};
		for (int i = 1; i < person.PossibleQNAResponses.Count; i++)
		{
			mangledQNA.Add(person.PossibleQNAResponses[i].Mangled());
		}

		return new()
		{
			// TODO: mangle
			Name = person.Name,
			// TODO: mangle
			Role = fakeRole,
			FakeRole = fakeRole,
			// skinwalkers are always a bit taller than their victims
			HeightInches = person.HeightInches + (mangleHeight ? Random.Range(6, 11) : Random.Range(1, 3)),
			// sometimes skinwalkers say a random floor
			AssignedFloors = new() {mangledFloor},
			// some qna responses are mangled
			PossibleQNAResponses = mangledQNA,
		};
	}
}

// interviews

public class InterviewResponses
{
	public string Name;
	public NpcRoles Role;
	public int HeightInches;

	public List<int> FloorsTheyveBeen;
	public List<QnA> QnA;

	//

	public static InterviewResponses FromPerson(Person person, int psychologists)
	{
		InterviewResponses responses = new()
		{
			Name = person.Name,
			Role = person.Role,
			HeightInches = person.HeightInches,
            FloorsTheyveBeen = new(),
            QnA = new(),
		};

		// assigned floors
		if (psychologists > 0)
		{
			responses.FloorsTheyveBeen = person.AssignedFloors;
			psychologists--;
		}

		// questioning
		if (psychologists > 0)
		{
			List<QnA> pool = new(person.PossibleQNAResponses);

			int numResponses = Mathf.Min(psychologists, pool.Count);

			for (int i = 0; i < numResponses; i++)
			{
				int idx = Random.Range(0, pool.Count);
				responses.QnA.Add(pool[idx]);
				pool.RemoveAt(idx);
			}
		}

		return responses;
	}
}

public class QnA
{
	public string Question;
	public string Response;
	public string BadResponse;

	//

	public QnA Mangled(float dropChance = 0.1f, float noMangleChance = 0.4f)
	{
		float sample = Random.value;

		bool shouldDrop = sample < dropChance;
		bool shouldMangle = sample < 1f - noMangleChance;

		return new()
		{
			Question = Question,
			Response = shouldDrop ? null : (shouldMangle ? BadResponse : Response),
			BadResponse = shouldMangle ? Response : BadResponse,
		};
	}
}

//
