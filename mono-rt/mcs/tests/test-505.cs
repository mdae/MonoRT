class T {
	public static int i;
	static int f ()
	{
		try {
		} finally {
			throw new System.Exception ("...");
		}
	}
	static void Main ()
	{
		try {
			i = f ();
		} catch {
			return;
		}
		throw new System.Exception ("error");
	}
}
