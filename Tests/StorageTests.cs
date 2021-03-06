﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emroy.Vfs.Service;
using Emroy.Vfs.Service.Enums;
using Emroy.Vfs.Service.Impl;
using Emroy.Vfs.Service.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service;

namespace Emroy.Vfs.Tests
{
    [TestClass]
    public class StorageTests
    {
        readonly IVfsDirectory _root = VfsDirectory.Root;

        [TestInitialize]
        public void SetUp()
        {

        }

        [TestMethod]
        public void TestCreateDirectoryAndFileInIt()
        {


            var directory = _root.CreateSubDirectory("dir1");
            directory.CreateFile("file2.txt");

            Assert.IsTrue(directory.Contains("file2.txt"));
            Assert.IsTrue(_root.Contains("dir1"));
        }

        [TestMethod]
        public void TestCreateDirectoryAndFileInItUsingPath()
        {

            var dir1 = _root.CreateSubDirectory("dir1");
            dir1.CreateFile("file2.txt");

            _root.CreateFile("dir1\\file22.txt");

            var dir2 = _root.CreateSubDirectory("dir1\\dir2");
            _root.CreateFile("dir1\\dir2\\file3.txt");
            Assert.IsTrue(dir1.Contains("file22.txt"));

            Assert.IsTrue(dir1.Contains("file22.txt"));
            Assert.IsTrue(dir1.Contains("dir2"), "dir1 does not contain dir2");

            Assert.IsTrue(dir2.Contains("file3.txt"), "dir2 does not contain file3");
        }


        [TestMethod]
        public void TestCopyDirectoryAndFiles()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            dir1.CreateFile("file2.txt");

            dir1.CreateFile("file1.txt");
            dir1.CreateFile("file12.txt");
            dir1.CreateFile("file123.txt");
            dir1.CreateFile("file1234.txt");
            dir1.CreateFile("file12345.txt");
            dir1.CreateFile("file123456.txt");

            _root.CreateFile("dir1\\file22.txt");

            _root.CreateSubDirectory("dir1\\dir2");
            Assert.IsTrue(dir1.Contains("file22.txt"));


            var dir11 = _root.CreateSubDirectory("dir11");
            var dir23 = dir11.CreateSubDirectory("dir23");
            var dir12 = _root.CreateSubDirectory("dir12");

            dir12.CreateSubDirectory("dir22");
            _root.CreateFile("dir12\\dir22\\file31.txt");
            _root.CopyEntity("dir1", "dir11\\dir23");


            Assert.IsTrue(dir23.Contains("dir1"));
            Assert.IsTrue((dir23 as VfsEntity).Parent.Name == "dir11");
            Assert.IsTrue(dir23.Contains(
                "dir1\\file1.txt", "dir1\\file12.txt", "dir1\\file123.txt",
                "dir1\\file1234.txt", "dir1\\file12345.txt", "dir1\\file123456.txt"));

            ShouldThrow( () => _root.CopyEntity("dir1\\file22.txt", "dir12\\dir22\\file31.txt"),
                "File got copied to a file! AAAA!!!", 
                VfsExceptionType.ObjAlreadyExists, VfsExceptionType.DirCorrespondToFile);

            ShouldThrow( () => _root.CopyEntity("dir1", "dir11\\dir23"),
                "Directory got copied twice! AAAAA!", VfsExceptionType.ObjAlreadyExists);


        }




        [TestMethod]
        public void TestMoveFileAndDirectory()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            dir1.CreateFile("file2.txt");
            var dir2 = dir1.CreateSubDirectory("dir2");
            dir2.CreateFile("file3.txt");
            dir2.CreateSubDirectory("dir3");

            _root.CreateFile("file1.txt");
            _root.MoveEntity(srcPath: "file1.txt", destPath: "dir1");
            _root.MoveEntity(srcPath:"dir1\\dir2",destPath:"");

            Assert.IsFalse(_root.Contains("file1.txt"));
            Assert.IsTrue(dir1.Contains("file1.txt"));
            Assert.IsTrue(_root.Contains("dir2"));
            Assert.IsFalse(dir1.Contains("dir2"));

        }


        [TestMethod]
        public void TestDeleteDirectoryWithSubdirectories()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            dir1.CreateSubDirectory("dir2");
            ShouldThrow(() => _root.DeleteSubDirectory("dir1", false),
                "Directory with subdirectories got deleted! AAAA!!", VfsExceptionType.DirContainsSubdirs);

        }


        [TestMethod]
        public void TestDeleteWithSubdirectories()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            var dir2 = dir1.CreateSubDirectory("dir2");
            dir2.CreateSubDirectory("dir3");
            _root.DeleteSubDirectory("dir1", true);

        }
        [TestMethod]
        public void TestDeleteLockedFile()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            dir1.CreateFile("file2.txt");

            var file1 = _root.CreateFile("file1.txt");
            _root.CreateFile("file123.txt");
            _root.CreateFile("file1234.txt");
            _root.CreateFile("file12345.txt");
            _root.CreateFile("file123456.txt");
            file1.LockFile("Antonio");

            ShouldThrow(() => _root.DeleteFile("file1.txt"), "Locked file got deleted! AAAA!",
                VfsExceptionType.CantDeleteLockedFile);



        }

        [TestMethod]
        public void TestDeleteLockedFileInDirectory()
        {
            var dir1 = _root.CreateSubDirectory("dir1");
            var dir2 = dir1.CreateSubDirectory("dir2");
            dir2.CreateFile("file3.txt");
            _root.LockFile("dir1\\dir2\\file3.txt", "default", true);
            dir2.CreateSubDirectory("dir3");
            ShouldThrow(() => _root.DeleteSubDirectory("dir1", true), 
             "Directory with locked files got deleted! AAAA!!", VfsExceptionType.DirContainLockedFiles);



        }


        [TestCleanup]
        public void TearDown()
        {
            _root.Clean();

        }
        public void ShouldThrow(Action action, string message, params VfsExceptionType[] expectedExceptions)
        {
            if (expectedExceptions.Length==0)
            {
                throw new ArgumentException("expectedExceptions.Length should be > 0!");
            }
            try
            {
                action();
                Assert.Fail(message);
            }
            catch (VfsException ex) { 
                if (!expectedExceptions.Contains(ex.ExType))
                {
                    Assert.Fail($"Expected exceptions are: {expectedExceptions.ToList()}, got {ex.ExType}");
                }

            }
        }
    }
}
