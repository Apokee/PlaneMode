public string GetSolution()
{
	var solutions = Directory.GetFiles(GetContext().Environment.WorkingDirectory.FullPath, "*.sln");

	if (solutions.Length == 1)
	{
		return solutions[0];
	}
	else
	{
		if (solutions.Length == 0)
		{
			throw new Exception("No solution found.");
		}
		else
		{
			throw new Exception("Multiple solutions found.");
		}
	}
}
