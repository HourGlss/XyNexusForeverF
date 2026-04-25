using System;
using Microsoft.Extensions.Logging;
using NexusForever.Game.Abstract.Chat;
using NexusForever.Game.Abstract.Entity;
using NexusForever.Network.Message;
using NexusForever.Network.World.Message.Model;
using NexusForever.Network.World.Message.Model.Chat;
using NexusForever.Game.Spell.Telemetry;
using NexusForever.WorldServer.Command;
using NexusForever.WorldServer.Command.Context;

namespace NexusForever.WorldServer.Network.Message.Handler.Chat
{
    public class ClientChatHandler : IMessageHandler<IWorldSession, ClientChat>
    {
        private const string CommandPrefix = "!";

        #region Dependency Injection

        private readonly ILogger<ClientChatHandler> log;
        private readonly IGlobalChatManager globalChatManager;
        private readonly ICommandManager commandManager;
        private readonly ISpellDiagnostics spellDiagnostics;

        public ClientChatHandler(
            ILogger<ClientChatHandler> log,
            IGlobalChatManager globalChatManager,
            ICommandManager commandManager,
            ISpellDiagnostics spellDiagnostics)
        {
            this.log               = log;
            this.globalChatManager = globalChatManager;
            this.commandManager    = commandManager;
            this.spellDiagnostics  = spellDiagnostics;
        }

        #endregion

        public void HandleMessage(IWorldSession session, ClientChat chat)
        {
            if (chat.Message.StartsWith(CommandPrefix))
                HandleCommand(session, chat);
            else
            {
                spellDiagnostics.RecordChatMessage(session.Player, chat.Channel.ChatChannelId, chat.Message);
                globalChatManager.HandleClientChat(session.Player, chat);
            }
        }

        private void HandleCommand(IWorldSession session, ClientChat chat)
        {
            try
            {
                session.EnqueueMessageEncrypted(new ServerChatAccept
                {
                    SenderName    = session.Player.Name,
                    ChatMessageId = chat.ChatMessageId
                });

                IWorldEntity target = null;
                if (session.Player.TargetGuid != null)
                    target = session.Player.GetVisible<IWorldEntity>(session.Player.TargetGuid.Value);

                var context = new WorldSessionCommandContext(session, target);
                commandManager.HandleCommand(context, chat.Message[CommandPrefix.Length..]);
            }
            catch (Exception e)
            {
                log.LogWarning($"{e.Message}: {e.StackTrace}");
            }
        }
    }
}
