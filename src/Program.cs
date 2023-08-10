// Preamble.exe prepends the specified string to the head of the specified file.
// Copyright (C) 2023 Timothy J. Bruce

/*
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

namespace Icod.Preamble {

	public static class Program {

		private const System.Int32 theBufferSize = 16384;


		[System.STAThread]
		public static System.Int32 Main( System.String[] args ) {
			if ( null == args ) {
				args = new System.String[ 0 ];
			}
			var len = args.Length;
			if ( ( 1 <= len ) && ( new System.String[] { "--copyright", "-c", "/c" }.Contains( args[ 0 ], System.StringComparer.OrdinalIgnoreCase ) ) ) {
				PrintCopyright();
				return 1;
			} else if ( ( len < 2 ) || ( 6 < len ) ) {
				PrintUsage();
				return 1;
			}
			--len;
			System.String? inputPathName = null;
			System.String? outputPathName = null;
			System.String? preamble = System.String.Empty;
			System.Boolean trim = false;
			System.String @switch;
			System.Int32 i = -1;
			do {
				@switch = args[ ++i ];
				if ( new System.String[] { "--help", "-h", "/h" }.Contains( inputPathName, System.StringComparer.OrdinalIgnoreCase ) ) {
					PrintUsage();
					return 1;
				} else if ( new System.String[] { "--copyright", "-c", "/c" }.Contains( inputPathName, System.StringComparer.OrdinalIgnoreCase ) ) {
					PrintCopyright();
					return 1;
				} else if ( "--input".Equals( @switch, System.StringComparison.OrdinalIgnoreCase ) ) {
					inputPathName = args[ ++i ].TrimToNull();
				} else if ( "--output".Equals( @switch, System.StringComparison.OrdinalIgnoreCase ) ) {
					outputPathName = args[ ++i ].TrimToNull();
				} else if ( "--preamble".Equals( @switch, System.StringComparison.OrdinalIgnoreCase ) ) {
					preamble = args[ ++i ];
				} else if ( "--trim".Equals( @switch, System.StringComparison.OrdinalIgnoreCase ) ) {
					trim = true;
					i++;
				} else {
					PrintUsage();
					return 1;
				}
			} while ( i < len );

			System.Func<System.String, System.String?> trimmer;
			if ( trim ) {
				trimmer = x => x.TrimToNull();
			} else {
				trimmer = x => x;
			}
			System.Func<System.String?, System.Collections.Generic.IEnumerable<System.String>> reader;
			if ( System.String.IsNullOrEmpty( inputPathName ) ) {
				reader = a => ReadStdIn( trimmer );
			} else {
				reader = a => ReadFile( a!, trimmer );
			}

			System.Action<System.String?, System.String, System.Collections.Generic.IEnumerable<System.String>> writer;
			if ( System.String.IsNullOrEmpty( outputPathName ) ) {
				writer = ( a, b, c ) => WriteStdOut( b, c );
			} else {
				writer = ( a, b, c ) => WriteFile( a!, b, c );
			}

			writer(
				outputPathName,
				preamble,
				reader( inputPathName )
			);
			return 0;
		}

		private static void PrintUsage() {
			System.Console.Error.WriteLine( "No, no, no! Use it like this, Einstein:" );
			System.Console.Error.WriteLine( "Preamble.exe --help" );
			System.Console.Error.WriteLine( "Preamble.exe --copyright" );
			System.Console.Error.WriteLine( "Preamble.exe --preamble thePreamble [--input inputFilePathName] [--output outputFilePathName]" );
			System.Console.Error.WriteLine( "Preamble.exe prepends the specified string to the head of the specified file." );
			System.Console.Error.WriteLine( "inputFilePathName and outputFilePathName may be relative or absolute paths." );
			System.Console.Error.WriteLine( "If inputFilePathName is omitted then input is read from StdIn." );
			System.Console.Error.WriteLine( "If outputFilePathName is omitted then output is written to StdOut." );
			System.Console.Error.WriteLine( "If --trim switch is specified, then input lines are trimmed of all surrounding whitespace and empty strings are ignored." );
		}
		private static void PrintCopyright() {
			var copy = new System.String[] {
				"Preamble.exe prepends the specified string to the head of the specified file.",
				"Copyright (C) 2023 Timothy J. Bruce",
				"",
				"",
				"This program is free software: you can redistribute it and / or modify",
				"it under the terms of the GNU General Public License as published by",
				"the Free Software Foundation, either version 3 of the License, or",
				"( at your option ) any later version.",
				"",
				"This program is distributed in the hope that it will be useful,",
				"but WITHOUT ANY WARRANTY; without even the implied warranty of",
				"MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the",
				"GNU General Public License for more details.",
				"",
				"You should have received a copy of the GNU General Public License",
				"along with this program.If not, see <https://www.gnu.org/licenses/>."
			};
			foreach ( var line in copy ) {
				System.Console.WriteLine( line );
			}
		}

		#region io
		private static System.Collections.Generic.IEnumerable<System.String> ReadStdIn( System.Func<System.String, System.String?> trimFunc ) {
			var line = System.Console.In.ReadLine();
			while ( null != line ) {
				line = trimFunc( line );
				if ( null != line ) {
					yield return line;
				}
				line = System.Console.In.ReadLine();
			}
		}
		private static System.Collections.Generic.IEnumerable<System.String> ReadFile( System.String? filePathName, System.Func<System.String, System.String?> trimFunc ) {
			filePathName = filePathName?.TrimToNull();
			if ( System.String.IsNullOrEmpty( filePathName ) ) {
				throw new System.ArgumentNullException( nameof( filePathName ) );
			}
			using ( var file = System.IO.File.Open( filePathName, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read ) ) {
				using ( var reader = new System.IO.StreamReader( file, System.Text.Encoding.UTF8, true, theBufferSize, true ) ) {
					var line = reader.ReadLine();
					while ( null != line ) {
						line = trimFunc( line );
						if ( null != line ) {
							yield return line;
						}
						line = reader.ReadLine();
					}
				}
			}
		}

		private static void WriteStdOut( System.String preamble, System.Collections.Generic.IEnumerable<System.String> data ) {
			System.Console.Out.WriteLine( preamble );
			foreach ( var datum in data ) {
				System.Console.Out.WriteLine( datum );
			}
		}
		private static void WriteFile( System.String? filePathName, System.String preamble, System.Collections.Generic.IEnumerable<System.String> data ) {
			filePathName = filePathName?.TrimToNull();
			if ( System.String.IsNullOrEmpty( filePathName ) ) {
				throw new System.ArgumentNullException( nameof( filePathName ) );
			}
			using ( var file = System.IO.File.Open( filePathName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write, System.IO.FileShare.None ) ) {
				_ = file.Seek( 0, System.IO.SeekOrigin.Begin );
				using ( var writer = new System.IO.StreamWriter( file, System.Text.Encoding.UTF8, theBufferSize, true ) ) {
					writer.WriteLine( preamble );
					foreach ( var datum in data ) {
						writer.WriteLine( datum );
					}
					writer.Flush();
					writer.Close();
				}
				file.Flush();
				file.SetLength( file.Position );
				file.Close();
			}
		}
		#endregion io

		private static System.String? TrimToNull( this System.String @string ) {
			if ( System.String.IsNullOrEmpty( @string ) ) {
				return null;
			}
			@string = @string.Trim();
			return System.String.IsNullOrEmpty( @string )
				? null
				: @string
			;
		}

	}

}