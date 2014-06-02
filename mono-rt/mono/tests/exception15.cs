using System;

class Tests {
	public static int Main(){
		int i = 0;
		try{
			try {
				throw new NotImplementedException();
			}
			finally {
				i++;
				Console.WriteLine("Finally called");
			}
		} catch(NotImplementedException e){
			//
			// Beginn Aenderung Test
			//
			Console.WriteLine(e.ToString());
			//
			// Ende Aenderung Test
			//
			i++;
			Console.WriteLine("Exception ignored");
		}

		if (i != 2)
			return 1;

		return 0;
	}
}
