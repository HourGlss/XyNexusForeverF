using System.Text;
using System.Xml;
using NexusForever.Network.Sts.Model;
using NexusForever.StsServer.Network.Packet;

namespace NexusForever.Tests;

public class StsProtocolTests
{
    [Fact]
    public void ConnectMessageReadsClientBodyFields()
    {
        var document = new XmlDocument();
        document.LoadXml("""
            <Connect>
              <ConnType>1</ConnType>
              <Process>2</Process>
              <ProductType>3</ProductType>
              <AppIndex>4</AppIndex>
              <Deployment>5</Deployment>
              <NotifyFlags>6</NotifyFlags>
              <VersionFlags>7</VersionFlags>
              <ConnProductType>8</ConnProductType>
              <ConnAppIndex>9</ConnAppIndex>
              <ConnDeployment>10</ConnDeployment>
              <ConnEpoch>11</ConnEpoch>
              <Address>127.0.0.1</Address>
              <Program>12</Program>
              <Build>13</Build>
              <Epoch>14</Epoch>
            </Connect>
            """);

        var message = new ClientConnectMessage();
        message.Read(document);

        Assert.Equal(1u, message.ConnType);
        Assert.Equal(2u, message.Process);
        Assert.Equal(3u, message.ProductType);
        Assert.Equal(4u, message.AppIndex);
        Assert.Equal(5u, message.Deployment);
        Assert.Equal(6u, message.NotifyFlags);
        Assert.Equal(7u, message.VersionFlags);
        Assert.Equal(8u, message.ConnProductType);
        Assert.Equal(9u, message.ConnAppIndex);
        Assert.Equal(10u, message.ConnDeployment);
        Assert.Equal(11u, message.ConnEpoch);
        Assert.Equal("127.0.0.1", message.Address);
        Assert.Equal(12u, message.Program);
        Assert.Equal(13u, message.Build);
        Assert.Equal(14u, message.Epoch);
    }

    [Fact]
    public void ConnectMessageReadsXStsConnectHeaderFields()
    {
        var message = new ClientConnectMessage();

        message.ReadHeaders(new Dictionary<string, string>
        {
            ["X-Sts-Connect"] = "1;product=2;index=3;address=127.0.0.1;deployment=4;epoch=5;notify=6;version=7;process=8;program=9;build=10"
        });

        Assert.Equal(1u, message.ConnType);
        Assert.Equal(2u, message.ProductType);
        Assert.Equal(3u, message.AppIndex);
        Assert.Equal("127.0.0.1", message.Address);
        Assert.Equal(4u, message.Deployment);
        Assert.Equal(5u, message.Epoch);
        Assert.Equal(6u, message.NotifyFlags);
        Assert.Equal(7u, message.VersionFlags);
        Assert.Equal(8u, message.Process);
        Assert.Equal(9u, message.Program);
        Assert.Equal(10u, message.Build);
    }

    [Fact]
    public void ClientStsPacketTrimsHeadersAndTreatsNamesCaseInsensitively()
    {
        byte[] data = Encoding.UTF8.GetBytes("POST /Sts/Connect STS/1.0\r\nL: 0\r\nX-Sts-Connect: 1;process=2\r\n\r\n");

        var packet = new ClientStsPacket(data);

        Assert.True(packet.Headers.ContainsKey("l"));
        Assert.True(packet.Headers.ContainsKey("x-sts-connect"));
        Assert.Equal("0", packet.Headers["l"]);
        Assert.Equal("1;process=2", packet.Headers["X-Sts-Connect"]);
    }

    [Fact]
    public void FragmentedPacketCompletesBodyAcrossMultipleFrames()
    {
        const string body = "<Request><LoginName>user@example.test</LoginName></Request>";
        string header = $"POST /Auth/LoginStart STS/1.0\r\nl:{body.Length}\r\n\r\n";
        byte[] first = Encoding.UTF8.GetBytes(header + body[..10]);
        byte[] second = Encoding.UTF8.GetBytes(body[10..]);

        var fragmented = new FragmentedStsPacket();
        PopulateAll(fragmented, first);

        Assert.True(fragmented.HasHeader);
        Assert.False(fragmented.HasBody);

        PopulateAll(fragmented, second);

        Assert.True(fragmented.HasBody);
        Assert.Equal(body, fragmented.GetPacket().Body);
    }

    private static void PopulateAll(FragmentedStsPacket packet, byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        while (reader.BaseStream.Position != reader.BaseStream.Length)
            packet.Populate(reader);
    }
}
