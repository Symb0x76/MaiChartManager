import { defineComponent, PropType, ref, computed, watch } from 'vue';
import { NFlex, NPopover } from 'naive-ui';
import { modInfo } from '@/store/refs';
import { PubKeyId, VerifyStatus } from '@/client/apiGen';

export default defineComponent({
  // props: {
  // },
  setup(props, { emit }) {

    return () => modInfo.value?.signature && <NPopover trigger="hover">
      {{
        trigger: () => modInfo.value?.signature?.status === VerifyStatus.Valid ?
          <div class="text-green-5 i-tabler:certificate text-2em" />
          : <div class="text-red-5 i-tabler:certificate-off text-2em" />,
        default: () => <NFlex vertical>
          {modInfo.value?.signature?.status === VerifyStatus.Valid && modInfo.value.signature?.keyId === PubKeyId.Local &&
            <div>已验证的 AquaMai 官方版本</div>}
          {modInfo.value?.signature?.status === VerifyStatus.Valid && modInfo.value.signature?.keyId === PubKeyId.CI &&
            <div>已验证的 AquaMai 官方持续集成构建</div>}
          {modInfo.value?.signature?.status === VerifyStatus.NotFound &&
            <div>这个 AquaMai 没有有效的签名，很可能不是官方版本</div>}
          {modInfo.value?.signature?.status === VerifyStatus.InvalidSignature &&
            <div>这个 AquaMai 的签名无效，很可能不是官方版本</div>}
        </NFlex>
      }}
    </NPopover>;
  },
});
