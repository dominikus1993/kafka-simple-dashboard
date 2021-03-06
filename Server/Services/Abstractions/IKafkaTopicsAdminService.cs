﻿using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaSimpleDashboard.Shared;
using LanguageExt;

namespace KafkaSimpleDashboard.Server.Services.Abstractions
{
    public interface IKafkaTopicsAdminService
    {
        ValueTask<Option<List<KafkaTopic>>> GetAll();
    }
}