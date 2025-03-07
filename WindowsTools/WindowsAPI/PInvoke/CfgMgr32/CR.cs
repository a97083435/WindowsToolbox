﻿namespace WindowsTools.WindowsAPI.PInvoke.CfgMgr32
{
    public enum CR
    {
        CR_SUCCESS = 0x00,
        CR_DEFAULT = 0x01,
        CR_OUT_OF_MEMORY = 0x02,
        CR_INVALID_POINTER = 0x03,
        CR_INVALID_FLAG = 0x04,
        CR_INVALID_DEVNODE = 0x05,
        CR_INVALID_DEVINST = CR_INVALID_DEVNODE,
        CR_INVALID_RES_DES = 0x06,
        CR_INVALID_LOG_CONF = 0x07,
        CR_INVALID_ARBITRATOR = 0x08,
        CR_INVALID_NODELIST = 0x09,
        CR_DEVNODE_HAS_REQS = 0x0a,
        CR_DEVINST_HAS_REQS = CR_DEVNODE_HAS_REQS,
        CR_INVALID_RESOURCEID = 0x0b,
        CR_DLVXD_NOT_FOUND = 0x0c,
        CR_NO_SUCH_DEVNODE = 0x0d,
        CR_NO_SUCH_DEVINST = CR_NO_SUCH_DEVNODE,
        CR_NO_MORE_LOG_CONF = 0x0e,
        CR_NO_MORE_RES_DES = 0x0f,
        CR_ALREADY_SUCH_DEVNODE = 0x10,
        CR_ALREADY_SUCH_DEVINST = CR_ALREADY_SUCH_DEVNODE,
        CR_INVALID_RANGE_LIST = 0x11,
        CR_INVALID_RANGE = 0x12,
        CR_FAILURE = 0x13,
        CR_NO_SUCH_LOGICAL_DEV = 0x14,
        CR_CREATE_BLOCKED = 0x15,
        CR_NOT_SYSTEM_VM = 0x16,
        CR_REMOVE_VETOED = 0x17,
        CR_APM_VETOED = 0x18,
        CR_INVALID_LOAD_TYPE = 0x19,
        CR_BUFFER_SMALL = 0x1a,
        CR_NO_ARBITRATOR = 0x1b,
        CR_NO_REGISTRY_HANDLE = 0x1c,
        CR_REGISTRY_ERROR = 0x1d,
        CR_INVALID_DEVICE_ID = 0x1e,
        CR_INVALID_DATA = 0x1f,
        CR_INVALID_API = 0x20,
        CR_DEVLOADER_NOT_READY = 0x21,
        CR_NEED_RESTART = 0x22,
        CR_NO_MORE_HW_PROFILES = 0x23,
        CR_DEVICE_NOT_THERE = 0x24,
        CR_NO_SUCH_VALUE = 0x25,
        CR_WRONG_TYPE = 0x26,
        CR_INVALID_PRIORITY = 0x27,
        CR_NOT_DISABLEABLE = 0x28,
        CR_FREE_RESOURCES = 0x29,
        CR_QUERY_VETOED = 0x2a,
        CR_CANT_SHARE_IRQ = 0x2b,
        CR_NO_DEPENDENT = 0x2c,
        CR_SAME_RESOURCES = 0x2d,
        CR_NO_SUCH_REGISTRY_KEY = 0x2e,
        CR_INVALID_MACHINENAME = 0x2f,
        CR_REMOTE_COMM_FAILURE = 0x30,
        CR_MACHINE_UNAVAILABLE = 0x31,
        CR_NO_CM_SERVICES = 0x32,
        CR_ACCESS_DENIED = 0x33,
        CR_CALL_NOT_IMPLEMENTED = 0x34,
        CR_INVALID_PROPERTY = 0x35,
        CR_DEVICE_INTERFACE_ACTIVE = 0x36,
        CR_NO_SUCH_DEVICE_INTERFACE = 0x37,
        CR_INVALID_REFERENCE_STRING = 0x38,
        CR_INVALID_CONFLICT_LIST = 0x39,
        CR_INVALID_INDEX = 0x3a,
        CR_INVALID_STRUCTURE_SIZE = 0x3b,
    }
}
