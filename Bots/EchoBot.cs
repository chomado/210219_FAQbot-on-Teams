// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.11.1

using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MyQnABot.Bots
{
    public class EchoBot : ActivityHandler
    {
        // コンストラクタで QnA Maker への接続情報と HttpClient を作るための IHttpClientFactory を受け取る
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public EchoBot(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        // ユーザーから話しかけられた時に呼ばれるメソッド。
        // なのでここで QnA Maker を呼び出す
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // QnA Maker に接続するためのクライアントを作る
            var qnaMaker = new QnAMaker(new QnAMakerEndpoint
                {
                    // appsetting.json に書いた設定項目 
                    KnowledgeBaseId = _configuration["QnAKnowledgebaseId"],
                    EndpointKey = _configuration["QnAEndpointKey"],
                    Host = _configuration["QnAEndpointHostName"]
                }, 
                options: null,
                httpClient: _httpClientFactory.CreateClient()
            );

            // QnA Maker から一番マッチした質問の回答を受け取る
            var options = new QnAMakerOptions { Top = 1 };
            var response = await qnaMaker.GetAnswersAsync(turnContext, options);

            // 回答が存在したら応答する
            if (response != null && response.Length > 0)
            {
                await turnContext.SendActivityAsync(
                        activity: MessageFactory.Text(response[0].Answer),
                        cancellationToken: cancellationToken
                    );
            }
            else
            {
                await turnContext.SendActivityAsync(
                        activity: MessageFactory.Text("質問に対する回答が見つかりませんでした"),
                        cancellationToken: cancellationToken
                    );
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "(*ﾟ▽ﾟ* っ)З こんにちは！何でも聞いてね";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
