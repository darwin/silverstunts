﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;

namespace SilverStunts
{
    public class EntityNamer
    {
        static int lastIndex = 0;

        public static string RequestName() { MoveNext(); return names[lastIndex]; }
        public static int Count() { return names.Length; }

        private static void MoveNext() 
        {
            lastIndex++;
            if (lastIndex == names.Length) lastIndex = 0;
        }

        private static string[] names = {
"mary",
"patricia",
"linda",
"barbara",
"elizabeth",
"jennifer",
"maria",
"susan",
"margaret",
"dorothy",
"lisa",
"nancy",
"karen",
"betty",
"helen",
"sandra",
"donna",
"carol",
"ruth",
"sharon",
"michelle",
"laura",
"sarah",
"kimberly",
"deborah",
"jessica",
"shirley",
"cynthia",
"angela",
"melissa",
"brenda",
"amy",
"anna",
"rebecca",
"virginia",
"kathleen",
"pamela",
"martha",
"debra",
"amanda",
"stephanie",
"carolyn",
"christine",
"marie",
"janet",
"catherine",
"frances",
"ann",
"joyce",
"diane",
"alice",
"julie",
"heather",
"teresa",
"doris",
"gloria",
"evelyn",
"jean",
"cheryl",
"mildred",
"katherine",
"joan",
"ashley",
"judith",
"rose",
"janice",
"kelly",
"nicole",
"judy",
"christina",
"kathy",
"theresa",
"beverly",
"denise",
"tammy",
"irene",
"jane",
"lori",
"rachel",
"marilyn",
"andrea",
"kathryn",
"louise",
"sara",
"anne",
"jacqueline",
"wanda",
"bonnie",
"julia",
"ruby",
"lois",
"tina",
"phyllis",
"norma",
"paula",
"diana",
"annie",
"lillian",
"emily",
"robin",
"james",
"john",
"robert",
"michael",
"william",
"david",
"richard",
"charles",
"joseph",
"thomas",
"christopher",
"daniel",
"paul",
"mark",
"donald",
"george",
"kenneth",
"steven",
"edward",
"brian",
"ronald",
"anthony",
"kevin",
"jason",
"matthew",
"gary",
"timothy",
"jose",
"larry",
"jeffrey",
"frank",
"scott",
"eric",
"stephen",
"andrew",
"raymond",
"gregory",
"joshua",
"jerry",
"dennis",
"walter",
"patrick",
"peter",
"harold",
"douglas",
"henry",
"carl",
"arthur",
"ryan",
"roger",
"joe",
"juan",
"jack",
"albert",
"jonathan",
"justin",
"terry",
"gerald",
"keith",
"samuel",
"willie",
"ralph",
"lawrence",
"nicholas",
"roy",
"benjamin",
"bruce",
"brandon",
"adam",
"harry",
"fred",
"wayne",
"billy",
"steve",
"louis",
"jeremy",
"aaron",
"randy",
"howard",
"eugene",
"carlos",
"russell",
"bobby",
"victor",
"martin",
"ernest",
"phillip",
"todd",
"jesse",
"craig",
"alan",
"shawn",
"clarence",
"sean",
"philip",
"chris",
"johnny",
"earl",
"jimmy",
"antonio"
};

    }
}
