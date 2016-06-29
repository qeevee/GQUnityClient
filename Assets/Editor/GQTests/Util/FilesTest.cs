﻿using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using GQ.Util;

namespace GQTests.Util {

	public class FilesTest {

		[Test]
		public void StripExtension () {
			//Assert
			Assert.AreEqual("filename", Files.StripExtension("filename.txt"), 
				"Should erase a '.txt' extension.");
			
			Assert.AreEqual("filename", Files.StripExtension("filename"), 
				"Should keep filename without any extension as it has been.");
			
			Assert.AreEqual("filename.txt", Files.StripExtension("filename.txt.png"), 
				"Should erase only the last extension.");
			Assert.AreEqual("filename", Files.StripExtension("filename."), 
				"Should erase a trailing '.' from filename as empty extension.");
			
			Assert.AreEqual("filename.", Files.StripExtension("filename..txt"), 
				"Should erase a '.txt' extension but keep additional dots before extension.");
			
			Assert.AreEqual(".filename", Files.StripExtension(".filename.txt"), 
				"Should erase a '.txt' also if filename starts with dot and has additional extension.");
			
			Assert.AreEqual(".", Files.StripExtension("."), 
				"Should keep single dot as filename.");
			
			Assert.AreEqual("..", Files.StripExtension(".."), 
				"Should keep double dot as filename.");
			
			Assert.AreEqual(".txt", Files.StripExtension(".txt"), 
				"Should keep filename if it starts with '.' and has no more dots");
		}

		[Test]
		public void Extension () {
			// Assert:
			Assert.AreEqual("txt", Files.Extension("filename.txt"), 
				"Should extract 'txt' from '.txt' extension.");
			
			Assert.AreEqual("", Files.Extension("filename"),
				"Should extract '' from filename without any extension.");
			
			Assert.AreEqual("png", Files.Extension("filename.txt.png"), 
				"Should extract the last extension if there are multiple.");
			
			Assert.AreEqual("", Files.Extension("filename."), 
				"Should extract '' from filename ending on a dot.");
			
			Assert.AreEqual("txt", Files.Extension("filename..txt"), 
				"Should ignore multiple dots separating the extension.");
			
			Assert.AreEqual("txt", Files.Extension(".filename.txt"), 
				"Should also find the extension if filename additionally starts with a dot.");
			
			Assert.AreEqual("", Files.Extension("."), 
				"Filename '.' has no extension.");
			
			Assert.AreEqual("", Files.Extension(".."), 
				"Filename '..' has no extension.");
			
			Assert.AreEqual("", Files.Extension(".txt"), 
				"Should ignore the extension because the filename otherwise would be empty.");
		}

		[Test]
		public void ExtensionSeparator () {
			// Assert:
			Assert.AreEqual(".", Files.ExtensionSeparator("filename.txt"));

			Assert.AreEqual("", Files.ExtensionSeparator("filename"));

			Assert.AreEqual(".", Files.ExtensionSeparator("filename.txt.png"));

			Assert.AreEqual(".", Files.ExtensionSeparator("filename."));

			Assert.AreEqual(".", Files.ExtensionSeparator("filename..txt"));

			Assert.AreEqual(".", Files.ExtensionSeparator(".filename.txt"));

			Assert.AreEqual("", Files.ExtensionSeparator("."));

			Assert.AreEqual("", Files.ExtensionSeparator(".."));

			Assert.AreEqual("", Files.ExtensionSeparator(".txt"));
		}

		[Test]
		public void ReassembleFilenames () {
			// Assert:
			Assert.AreEqual("filename.txt", reassemble("filename.txt"));

			Assert.AreEqual("filename", reassemble("filename"));

			Assert.AreEqual("filename.txt.png", reassemble("filename.txt.png"));

			Assert.AreEqual("filename.", reassemble("filename."));

			Assert.AreEqual("filename..txt", reassemble("filename..txt"));

			Assert.AreEqual(".filename.txt", reassemble(".filename.txt"));

			Assert.AreEqual(".", reassemble("."));

			Assert.AreEqual("..", reassemble(".."));

			Assert.AreEqual(".txt", reassemble(".txt"));
		}

		private string reassemble (string filename) {
			return Files.StripExtension(filename) + Files.ExtensionSeparator(filename) + Files.Extension(filename);
		}
	}
}
