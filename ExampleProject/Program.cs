﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            System.Console.Out.WriteLine(new Shapp.ShappAPI().Elo());
        }
    }
}