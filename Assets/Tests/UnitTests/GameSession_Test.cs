using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using PsychOutDestined;
using UnityEngine;
using UnityEngine.TestTools;
//using Moq;

public class GameSession_Test
{
    GameSession gameSession;
    SerializableUnit blankUnit;

    List<SerializableUnit> testParty2;
    //Mock<JsonDataService> mockDataService;

    public GameSession_Test()
    {
        gameSession = new GameSession();

        blankUnit = new SerializableUnit();

        testParty2 = new List<SerializableUnit>();
    }

    [Test]
    public void GameSession_TestSimplePasses()
    {
        List<SerializableUnit> partyOverSizeLimit = new List<SerializableUnit>();
        for(int i = 0; i < GameSession.MAX_PARTY_SIZE; i++) 
            partyOverSizeLimit.Add(blankUnit);
        
    }
}
