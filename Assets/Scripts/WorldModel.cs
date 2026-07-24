using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
			PossibleQNAResponses = genInfo.RandomQnA(3),
		};
	}
}

public class Skinwalker : Person
{
	public override bool IsSkinwalker => true;
	public List<Person> Victims;

	//

	// TODO: a smarter/cooler skinwalker mangling system
	// - we should be deliberate about what gets mangled and what doesn't, for challenge
	// - smarter mangling, based on context abt floors and stuff
	public static Skinwalker FromPerson(Person person)
	{
		return new()
		{
			// TODO: mangle
			Name = person.Name,
			// TODO: mangle
			Role = person.Role,
			// skinwalkers are always a bit taller than their victims
			HeightInches = person.HeightInches + Random.Range(1, 4),
			// sometimes skinwalkers say a random floor
			AssignedFloors =
				Random.value < 0.2f ? new() { UnityEngine.Random.Range(1, 10) } : new(person.AssignedFloors),
			// some qna responses are mangled
			PossibleQNAResponses = person.PossibleQNAResponses.Select(qna => qna.Mangled()).ToList(),
		};
	}
}

// interviews

public class InterviewResponses
{
    public Person Source;
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
            Source = person,
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
