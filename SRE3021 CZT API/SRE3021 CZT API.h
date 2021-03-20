#pragma once
#include <IDEAS_packetDecode.h>
#include <IDEAS_packetEncode.h>
#include <IDEAS_packetTypes.h>

using namespace System;

namespace HUREL {
namespace Compton{
namespace SRE3021{
	public ref class IDEASPacketLibWrapper
	{
    public:
        void EncodeHeader(
            uint8_t         version,
            uint8_t         system_num,
            uint8_t         packet_type,
            IDEAS_packetSeq seq_flags,
            uint16_t        packet_count,
            uint32_t        timestamp,
            uint16_t        data_len,
            uint8_t* pEncBuffer
        );
	};
	};
	}
	}
}
