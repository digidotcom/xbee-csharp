/*
 * Copyright 2019, Digi International Inc.
 * 
 * Permission to use, copy, modify, and/or distribute this software for any
 * purpose with or without fee is hereby granted, provided that the above
 * copyright notice and this permission notice appear in all copies.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 * WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 * ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 * WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 * ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 * OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.
 */

namespace ConsoleApp.Communication.IP.KnockKnockSample
{
	public class KnockKnockProtocol
	{
		//Constants.

		private const int WAITING = 0;
		private const int SENTKNOCKKNOCK = 1;
		private const int SENTCLUE = 2;
		private const int ANOTHER = 3;
		private const int NUMJOKES = 5;

		//Variables.

		private int state = WAITING;
		private int currentJoke = 0;

		private string[] clues = { "Turnip",
								"Little Old Lady",
								"Atch",
								"Who",
								"Who" };
		private string[] answers = { "Turnip the heat, it's cold in here!",
								 "I didn't know you could yodel!",
								 "Bless you!",
								 "Is there an owl in here?",
								 "Is there an echo in here?" };

		public string ProcessInput(string theInput)
		{
			string theOutput = null;

			if (state == WAITING)
			{
				theOutput = "Knock! Knock!";
				state = SENTKNOCKKNOCK;
			}
			else if (state == SENTKNOCKKNOCK)
			{
				if (theInput.ToLower().Equals("who's there?"))
				{
					theOutput = clues[currentJoke];
					state = SENTCLUE;
				}
				else
				{
					theOutput = "You're supposed to say \"Who's there?\"! " +
					"Try again. Knock! Knock!";
				}
			}
			else if (state == SENTCLUE)
			{
				if (theInput.ToLower().Equals(clues[currentJoke].ToLower() + " who?"))
				{
					theOutput = answers[currentJoke] + " Want another? (y/n)";
					state = ANOTHER;
				}
				else
				{
					theOutput = "You're supposed to say \"" +
					clues[currentJoke] +
					" who?\"" +
					"! Try again. Knock! Knock!";
					state = SENTKNOCKKNOCK;
				}
			}
			else if (state == ANOTHER)
			{
				if (theInput.ToLower().Equals("y"))
				{
					theOutput = "Knock! Knock!";
					if (currentJoke == (NUMJOKES - 1))
						currentJoke = 0;
					else
						currentJoke++;
					state = SENTKNOCKKNOCK;
				}
				else
				{
					theOutput = "Bye.";
					state = WAITING;
				}
			}
			return theOutput;
		}
	}
}
