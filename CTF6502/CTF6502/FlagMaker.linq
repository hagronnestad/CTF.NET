<Query Kind="Program" />

void Main()
{
	var flag = "";
	byte key = 0;
	
	var s1 = Xor(flag, key);
	s1.Dump();
	WriteBytes(s1);

	var s2 = Xor(s1, key);
	s2.Dump();
	WriteBytes(s2);
}

// Define other methods and classes here
string Xor(string s, byte key)
{
	var r = new StringBuilder();
	foreach (var c in s)
	{
		r.Append((char)(c ^ key));
	}

	return r.ToString();
}

void WriteBytes(string s)
{
	foreach (var c in s.ToString())
	{
		Debug.Write($"0x{(int)c:X2},");
	}
	Debug.WriteLine("");
}