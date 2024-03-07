using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using PsychOutDestined;
using UnityEngine;
using UnityEngine.TestTools;
using Moq;

public class GameSession_Test
{
    GameSession gameSession;
    SerializableUnit blankUnit;

    List<SerializableUnit> blankUnitParty;
    Mock<IDataService> mockDataService;

    public GameSession_Test()
    {
        gameSession = new GameSession();
        //gameSession.dataService

        blankUnit = new SerializableUnit();

        blankUnitParty = new List<SerializableUnit>() {blankUnit, blankUnit, blankUnit};

        mockDataService = new Mock<IDataService>();
        SetupDataServiceMock();
    }

    public void SetupDataServiceMock()
    {
        mockDataService.Setup(x => x.LoadData(
                            It.IsAny<string>(), 
                            It.IsAny<List<SerializableUnit>>(), 
                            It.IsAny<bool>())).Returns(blankUnitParty);

        mockDataService.Setup(x => x.LoadData(
                            It.IsAny<string>(), 
                            It.IsAny<Vector3>(), 
                            It.IsAny<bool>())).Returns(Vector3.zero);
        
        mockDataService.Setup(x => x.LoadData(
                            It.IsAny<string>(), 
                            It.IsAny<string>(), 
                            It.IsAny<bool>())).Returns("");
    }

    // [Test]
    // public void GameSession_TestPartyLimitSize()
    // {
    //     SetupDataServiceMock();

    //     List<SerializableUnit> partyOverSizeLimit = new List<SerializableUnit>();
    //     for(int i = 0; i < GameSession.MAX_PARTY_SIZE + 5; i++) 
    //         partyOverSizeLimit.Add(blankUnit);
        
    //     mockDataService.Setup(x => x.LoadData(
    //                         It.IsAny<string>(), 
    //                         It.IsAny<List<SerializableUnit>>(), 
    //                         It.IsAny<bool>())).Returns(partyOverSizeLimit);
        
    //     gameSession.LoadSaveFiles();
    //     Assert.IsTrue(gameSession.playerParty.Count == GameSession.MAX_PARTY_SIZE);
    // }
}
