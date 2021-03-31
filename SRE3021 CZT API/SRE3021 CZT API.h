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
	enum class PacketType : int {
			Unknown = 0,
			WriteSysReg = 0x10,
			ReadSysReg = 0x11,
			ReadBackSysReg = 0x12,
			ReadWriteASICConfig = 0xC0,
			ReadBackASCIConfig = 0xC1,
			ImageData = 0xD1
	};
	PacketType GetPacketType();
	

		
	};



	}
	}
	}

