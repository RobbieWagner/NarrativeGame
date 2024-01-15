using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ink.Runtime;

namespace RobbieWagnerGames
{
   public static class DialogueConfigurer
   {
      public static Story ConfigureStory(TextAsset text)
      {
         Story newStory = new Story(text.text);
         return newStory;
      }
   }
}
