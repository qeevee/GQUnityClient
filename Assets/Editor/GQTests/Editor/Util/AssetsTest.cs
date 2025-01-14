﻿using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.IO;
using GQ.Editor.Util;

namespace GQTests.Editor.Util
{

	public class AssetsTest
	{

		private static string ASSETS_TEST_DIR = Files.CombinePath (GQAssert.TEST_DATA_BASE_DIR, "AssetsTest");
		private static string EMPTY_DIR = Files.CombinePath (ASSETS_TEST_DIR, "EmptyFolder");
		private static string TARGET_DIR = Files.CombinePath (ASSETS_TEST_DIR, "Targets");
		private static string GIVEN_ASSETS_DIR = Files.CombinePath (ASSETS_TEST_DIR, "GivenAssets");


		[SetUp]
		public void SetUp ()
		{
			if (!Files.ExistsDir (EMPTY_DIR))
				Files.CreateDir (EMPTY_DIR);
			GQ.Editor.Util.Assets.ClearAssetFolder (EMPTY_DIR);

			if (!Files.ExistsDir (TARGET_DIR))
				Files.CreateDir (TARGET_DIR);
			GQ.Editor.Util.Assets.ClearAssetFolder (TARGET_DIR);
		}

		[TearDown]
		public void TearDown ()
		{
			GQ.Editor.Util.Assets.ClearAssetFolder (EMPTY_DIR);
			GQ.Editor.Util.Assets.ClearAssetFolder (TARGET_DIR);
		}



		[Test]
		public void AbsolutePath ()
		{
			// Arrange:
			string relPath = Files.CombinePath ("Assets", "MySubdir", "FurtherSubDir");
			string absPath = Files.CombinePath (Application.dataPath, "MySubdir", "FurtherSubDir");

			// Act & Assert:
			Assert.AreEqual (absPath, GQ.Editor.Util.Assets.AbsolutePath4Asset (relPath));
			Assert.AreEqual (absPath, GQ.Editor.Util.Assets.AbsolutePath4Asset (absPath), "AbsolutePath() should not change absolute paths.");
		}



		[Test]
		public void RelativeAssetPath ()
		{
			// Arrange:
			string relPath = Files.CombinePath ("Assets", "MySubdir", "FurtherSubDir");
			string absPath = Files.CombinePath (Application.dataPath, "MySubdir", "FurtherSubDir");

			// Assert:
			Assert.AreEqual (
				relPath,
				GQ.Editor.Util.Assets.RelativeAssetPath (absPath),
				"RelativeAssetPath() should strip the Application.datapath part from an absolute asset path."
			);
			Assert.AreEqual (
				"Assets/path/to/an/asset", 
				GQ.Editor.Util.Assets.RelativeAssetPath (Files.CombinePath (Application.dataPath, "path/to/an/asset")));
			Assert.AreEqual (
				"Assets/path/to/an/asset",
				GQ.Editor.Util.Assets.RelativeAssetPath ("Assets/path/to/an/asset"),
				"RelativeAssetPath() should not change relative assets paths."
			);
		}

		[Test]
		public void CreateFolder ()
		{
			// Arrange:
			string emptyDir = Files.CombinePath (ASSETS_TEST_DIR, "EmptyFolder");
			string newDir = Files.CombinePath (emptyDir, "NewDir");

			// Pre Assert:
			Assert.That (new System.IO.DirectoryInfo (emptyDir), Is.Empty);

			// Act:
			AssetDatabase.CreateFolder (emptyDir, "NewDir");
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();

			// Assert:
			Assert.That (new System.IO.DirectoryInfo (emptyDir), Is.Not.Empty);
			Assert.That (Directory.Exists (newDir));
			Assert.That (GQ.Editor.Util.Assets.ExistsAssetAtPath (newDir));
		}


		/// <summary>
		/// This test is made just to comprehend the AssetBatabase and its file creating functions.
		/// </summary>
		[Test]
		public void ExistsAsset ()
		{
			// Arrange:
//			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			string newAssetPath = Files.CombinePath (EMPTY_DIR, "Text.txt");

			// Pre Assert:
			Assert.That (!File.Exists (newAssetPath));
			Assert.That (!GQ.Editor.Util.Assets.ExistsAssetAtPath (newAssetPath));

			// Act Create:
			// TODO change to a real Method in Files
			using (StreamWriter sw = File.CreateText (GQ.Editor.Util.Assets.AbsolutePath4Asset (newAssetPath))) {
				sw.WriteLine ("Some text content.");
				sw.Close ();
			}

			AssetDatabase.Refresh ();

			// Assert:
			Assert.That (File.Exists (newAssetPath));
			Assert.That (GQ.Editor.Util.Assets.ExistsAssetAtPath (newAssetPath));

			// Act Delete:
			AssetDatabase.DeleteAsset (newAssetPath);

			// Post Assert:
			Assert.That (!File.Exists (newAssetPath));
			Assert.That (!GQ.Editor.Util.Assets.ExistsAssetAtPath (newAssetPath));
		}

		[Test]
		public void ExistsAssetAtPath ()
		{
			// Arrange relative asset paths:
			string relPathToExistingAssetsFile = 
				Files.CombinePath (GQAssert.TEST_DATA_BASE_DIR, "AssetsTest", "GivenAssets", "Image.png");
			string relPathToExistingAssetsDir = 
				Files.CombinePath (GQAssert.TEST_DATA_BASE_DIR, "AssetsTest", "EmptyFolder");
			string relPathToNonExistingAssetsFile = 
				Files.CombinePath (GQAssert.TEST_DATA_BASE_DIR, "AssetsTest", "GivenAssets", "DoesNotExi.st");

			// Assert:
			Assert.That (
				GQ.Editor.Util.Assets.ExistsAssetAtPath (relPathToExistingAssetsFile), 
				"Existing asset file should be validated by relative path.");
			Assert.That (
				GQ.Editor.Util.Assets.ExistsAssetAtPath (relPathToExistingAssetsDir), 
				"Existing asset dir should be validated by relative path.");
			Assert.That (
				!GQ.Editor.Util.Assets.ExistsAssetAtPath (relPathToNonExistingAssetsFile), 
				"Non-Existing asset file should NOT be validated by relative path.");


			// Arrange absolute asset paths:
			string absPathToExistingAssetsFile = 
				Files.CombinePath (Application.dataPath, "Editor", "GQTestsData", "AssetsTest", "GivenAssets", "Image.png");
			string absPathToExistingAssetsDir = 
				Files.CombinePath (Application.dataPath, "Editor", "GQTestsData", "AssetsTest", "EmptyFolder");
			string absPathToNonExistingAssetsFile = 
				Files.CombinePath (Application.dataPath, "Editor", "GQTestsData", "AssetsTest", "GivenAssets", "DoesNotExi.st");
			
			// Assert:
			Assert.That (
				GQ.Editor.Util.Assets.ExistsAssetAtPath (absPathToExistingAssetsFile), 
				"Existing asset file should be validated by absolute path.");
			Assert.That (
				GQ.Editor.Util.Assets.ExistsAssetAtPath (absPathToExistingAssetsDir), 
				"Existing asset dir should be validated by absolute path.");
			Assert.That (
				!GQ.Editor.Util.Assets.ExistsAssetAtPath (absPathToNonExistingAssetsFile), 
				"Non-Existing asset file should NOT be validated by absolute path.");
					
			// Arrange NON asset paths:
			string pathToExistingNonAssetsFile = 
				Files.CombinePath (GQAssert.PROJECT_PATH, "TestsData", "NonEmptyDir", "Image.png");
			string pathToExistingNonAssetsDir = 
				Files.CombinePath (GQAssert.TEST_DATA_BASE_DIR, "TestsData", "EmptyDir");
			string pathToNonExistingNonAssetsFile = 
				Files.CombinePath (GQAssert.TEST_DATA_BASE_DIR, "TestsData", "EmptyDir", "DoesNotExi.st");

			// Assert:
			Assert.That (
				!GQ.Editor.Util.Assets.ExistsAssetAtPath (pathToExistingNonAssetsFile), 
				"Existing non asset file should not be validated by path.");
			Assert.That (
				!GQ.Editor.Util.Assets.ExistsAssetAtPath (pathToExistingNonAssetsDir), 
				"Existing non asset dir should not be validated by path.");
			Assert.That (
				!GQ.Editor.Util.Assets.ExistsAssetAtPath (pathToNonExistingNonAssetsFile), 
				"Non-Existing non asset file should NOT be validated by path.");
		}

		[Test]
		public void IsAssetPath ()
		{
			// Arrange relative asset paths:
			string relPathToExistingAssetsFile = 
				Files.CombinePath (GQAssert.TEST_DATA_BASE_DIR, "AssetsTest", "GivenAssets", "Image.png");
			string relPathToExistingAssetsDir = 
				Files.CombinePath (GQAssert.TEST_DATA_BASE_DIR, "AssetsTest", "EmptyFolder");
			string relPathToNonExistingAssetsFile = 
				Files.CombinePath (GQAssert.TEST_DATA_BASE_DIR, "AssetsTest", "GivenAssets", "DoesNotExi.st");

			// Assert:
			Assert.That (GQ.Editor.Util.Assets.IsAssetPath (relPathToExistingAssetsFile), "Relative path to an existing asset file should be recognized by IsAssetPath()");
			Assert.That (GQ.Editor.Util.Assets.IsAssetPath (relPathToExistingAssetsDir), "Relative path to an existing asset dir should be recognized by IsAssetPath()");
			Assert.That (
				GQ.Editor.Util.Assets.IsAssetPath (relPathToNonExistingAssetsFile), 
				"Relative path to a NON existing asset file should STILL be recognized by IsAssetPath(); " + relPathToNonExistingAssetsFile);

			// Arrange absolute asset paths:
			string absPathToExistingAssetsFile = 
				Files.CombinePath (Application.dataPath, "Editor", "GQTestsData", "AssetsTest", "GivenAssets", "Image.png");
			string absPathToExistingAssetsDir = 
				Files.CombinePath (Application.dataPath, "Editor", "GQTestsData", "AssetsTest", "EmptyFolder");
			string absPathToNonExistingAssetsFile = 
				Files.CombinePath (Application.dataPath, "Editor", "GQTestsData", "AssetsTest", "GivenAssets", "DoesNotExi.st");

			// Assert:
			Assert.That (GQ.Editor.Util.Assets.IsAssetPath (absPathToExistingAssetsFile), "Absolute path to an existing asset file should be recognized by IsAssetPath()");
			Assert.That (GQ.Editor.Util.Assets.IsAssetPath (absPathToExistingAssetsDir), "Absolute path to an existing asset dir should be recognized by IsAssetPath()");
			Assert.That (GQ.Editor.Util.Assets.IsAssetPath (absPathToNonExistingAssetsFile), "Absolute path to a NON existing asset file should NOT be recognized by IsAssetPath()");

			// Arrange NON-Asset paths:
			string pathToExistingNonAssetsFile = 
				Files.CombinePath (GQAssert.PROJECT_PATH, "TestsData", "NonEmptyDir", "Image.png");
			string pathToExistingNonAssetsDir = 
				Files.CombinePath (GQAssert.PROJECT_PATH, "TestsData", "EmptyDir");
			string pathToNonExistingNonAssetPath = 
				Files.CombinePath (GQAssert.PROJECT_PATH, "TestsData", "EmptyDir", "DoesNotExi.st");
			

			// Assert:
			Assert.That (!GQ.Editor.Util.Assets.IsAssetPath (pathToExistingNonAssetsFile), "Path to an existing file outside of Assets should NOT be recognized by IsAssetPath()");
			Assert.That (!GQ.Editor.Util.Assets.IsAssetPath (pathToExistingNonAssetsDir), "Path to an existing dir outside of Assets should NOT be recognized by IsAssetPath()");
			Assert.That (!GQ.Editor.Util.Assets.IsAssetPath (pathToNonExistingNonAssetPath), "Path to an non-existing file / dir outside of Assets should NOT be recognized by IsAssetPath()");
		}

		private void AssertThatAllGivenAssetsExistInDir (string dir)
		{

			DirectoryInfo givenAssetsDir = new DirectoryInfo (GIVEN_ASSETS_DIR);
			foreach (FileInfo givenFile in givenAssetsDir.GetFiles()) {
				if (givenFile.Name.EndsWith (".meta"))
					continue;

				var targetFilePathRel = Files.CombinePath (dir, givenFile.Name);
				var targetFilePathAbs = GQ.Editor.Util.Assets.AbsolutePath4Asset (targetFilePathRel);
				Assert.That (
					File.Exists (targetFilePathAbs), 
					"File should have been copied to: " + targetFilePathAbs);
				Assert.That (
					GQ.Editor.Util.Assets.ExistsAssetAtPath (targetFilePathRel), 
					"Asset should have been copied to: " + targetFilePathRel);
			}
		}


		[Test]
		public void ClearAssetsFolder ()
		{
			// Arrange: we place some iven assets into a new directory so that we can clear that directory and check that clearing works:
			var newDir = Files.CombinePath (EMPTY_DIR, "newDir");
			var newDirInfo = new DirectoryInfo (newDir);

			Directory.CreateDirectory (GQ.Editor.Util.Assets.AbsolutePath4Asset (newDir));
			GQ.Editor.Util.Assets.copyAssetsDir (GIVEN_ASSETS_DIR, newDir);

			// Act:
			GQ.Editor.Util.Assets.ClearAssetFolder (newDir);

			// Assert:
			Assert.That (newDirInfo, Is.Empty);

			foreach (var givenFile in newDirInfo.GetFiles()) {
				if (givenFile.Name.EndsWith (".meta"))
					continue;

				var targetFilePathRel = Files.CombinePath (newDir, givenFile.Name);
				var targetFilePathAbs = 
					GQ.Editor.Util.Assets.AbsolutePath4Asset (targetFilePathRel);
				Assert.That (
					!File.Exists (targetFilePathAbs), 
					"File should have been deleted: " + targetFilePathAbs);
				Assert.That (
					!GQ.Editor.Util.Assets.ExistsAssetAtPath (targetFilePathRel), 
					"Asset should have been deleted: " + targetFilePathRel);
			}
		}

	}
}
