﻿namespace Server_API.Domain.Service.InfrastrutureService.Interface
{
    public interface IEncryptionService
    {
        public string Encrypt(string data);

        public string Decrypt(string data);
    }
}