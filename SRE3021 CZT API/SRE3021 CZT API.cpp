#include "pch.h"

#include "SRE3021 CZT API.h"

void HUREL::Compton::SRE3021::IDEASPacketLibWrapper::EncodeHeader(uint8_t version, uint8_t system_num, uint8_t packet_type, IDEAS_packetSeq seq_flags, uint16_t packet_count, uint32_t timestamp, uint16_t data_len, uint8_t* pEncBuffer)
{
    IDEAS_encode_header(version,system_num,
        packet_type,
        seq_flags,
        packet_count,
        timestamp,
        data_len,
        pEncBuffer);
}
