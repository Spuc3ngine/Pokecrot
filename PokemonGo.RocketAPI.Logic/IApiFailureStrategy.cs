using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using POGOProtos.Networking.Envelopes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Logic
{
    public class ApiFailureStrategy : IApiFailureStrategy
    {
        private readonly Client _session;
        private int _retryCount;

        public ApiFailureStrategy(Client session)
        {
            _session = session;
        }

        public async Task<ApiOperation> HandleApiFailure()
        {
            if (_retryCount == 11)
                return ApiOperation.Abort;

            await Task.Delay(500);
            _retryCount++;

            if (_retryCount % 5 == 0)
            {
                DoLogin();
            }

            return ApiOperation.Retry;
        }

        public void HandleApiSuccess()
        {
            _retryCount = 0;
        }

        private async void DoLogin()
        {
            try
            {
                if (_session.Settings.AuthType != AuthType.Google || _session.Settings.AuthType != AuthType.Ptc)
                {
                    await _session.Login.DoLogin();
                }
                else
                {
                    Logger.Error("Wrong Auth Type?");
                }
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten().InnerException;
            }
            catch (LoginFailedException)
            {
                Logger.Error("Failed to login?");
            }
            catch (AccessTokenExpiredException)
            {
                Logger.Error("AccessTokenExpired? Retrying in 1 Second.");

                await Task.Delay(1000);
            }
            catch (PtcOfflineException)
            {
                Logger.Error("PTC Offline retrying in 15 seconds.");
                await Task.Delay(15000);
            }
            catch (InvalidResponseException)
            {
                Logger.Error("Invalid Response exception, retrying in 5 seconds.");

                await Task.Delay(5000);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }
        public void HandleApiSuccess(RequestEnvelope request, ResponseEnvelope response)
        {
            _retryCount = 0;
        }

        public async Task<ApiOperation> HandleApiFailure(RequestEnvelope request, ResponseEnvelope response)
        {
            if (_retryCount == 11)
                return ApiOperation.Abort;

            await Task.Delay(500);
            _retryCount++;

            if (_retryCount % 5 == 0)
            {
                try
                {
                    DoLogin();
                }
                catch (PtcOfflineException)
                {
                    await Task.Delay(20000);
                }
                catch (AccessTokenExpiredException)
                {
                    await Task.Delay(2000);
                }
                catch (Exception ex) when (ex is InvalidResponseException || ex is TaskCanceledException)
                {
                    await Task.Delay(1000);
                }
            }

            return ApiOperation.Retry;
        }
    }
}