using _200SXContact.Interfaces.Areas.Admin;
using _200SXContact.Models.DTOs.Areas.Newsletter;
using AutoMapper;
using MediatR;

namespace _200SXContact.Queries.Areas.Newsletter
{
    public class GetCreateNewsletterViewQuery : IRequest<NewsletterDto> {}
    public class GetCreateNewsletterViewQueryHandler : IRequestHandler<GetCreateNewsletterViewQuery, NewsletterDto>
    {
        private readonly ILoggerService _loggerService;
        private readonly IMapper _mapper;
        public GetCreateNewsletterViewQueryHandler(ILoggerService loggerService, IMapper mapper)
        {
            _loggerService = loggerService;
            _mapper = mapper;
        }
        public Task<NewsletterDto> Handle(GetCreateNewsletterViewQuery request, CancellationToken cancellationToken)
        {
            _loggerService.LogAsync("Newsletter || Getting create newsletter admin page", "Info", "");

            var model = new NewsletterDto
            {
                Subject = "Insert Subject Here",
                Body = @"<!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <style>
                        body {
                            font-family: 'Helvetica', 'Roboto', sans-serif;
                            margin: 0;
                            padding: 0;
                            background-color: #2c2c2c; 
                            color: #ffffff;
                        }
                        .container {
                            width: 100%;
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 20px;
                            background-color: #3c3c3c;
                            border-radius: 8px;
                            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
                        }
                        .header {
                            text-align: center;
                        }
                        .header img {
                            max-width: 100%;
                            height: auto;
                            border-radius: 8px;
                        }
                        h1 {
                            color: #f5f5f5;
                            font-size: 24px;
                            margin: 20px 0;
                        }
                        p {
                            line-height: 1.6;
                            margin: 10px 0;
                            color: #f5f5f5;
                        }
                        .button {
                            display: inline-block;
                            padding: 10px 20px;
                            font-size: 16px;
                            font-weight: bold;
                            color: #ffffff;
                            background-color: #d0bed1;
                            text-decoration: none;
                            border-radius: 5px;
                            transition: background-color 0.3s ease;
                        }
                        .button:hover {
                            background-color: #966b91;
                        }
                        .footer {
                            text-align: center;
                            margin-top: 20px;
                            font-size: 12px;
                            color: #b0b0b0;
                        }
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <a href=""https://www.200sxproject.com"" target=""_blank"">
								<img src=""https://200sxproject.com/images/verifHeader.JPG"" alt=""200SX Project"" />
							</a>
                        </div>
                        <h1>title of article</h1>
                        <p>Hi there,</p>
                        <p>ADD THE TEXT HEREEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE</p>                            
						<p>
							<a href='https://www.200sxproject.com/newsletter/unsubscribe?email={EMAIL}' class='unsubscribe'>Unsubscribe here</a>
						</p>
                        <div class='footer'>
                            <p>© 2025 200SX Project. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>"
            };

            NewsletterDto dto = _mapper.Map<NewsletterDto>(model);

            _loggerService.LogAsync("Newsletter || Got create newsletter admin page", "Info", "");

            return Task.FromResult(dto);
        }
    }
}
