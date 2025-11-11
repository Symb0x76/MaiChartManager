import { defineComponent, PropType, ref, computed, watch } from 'vue';
import { NFlex, NPopover } from 'naive-ui';
import { modInfo } from '@/store/refs';
import { PubKeyId, VerifyStatus } from '@/client/apiGen';
import { useI18n } from 'vue-i18n';

export default defineComponent({
  // props: {
  // },
  setup(props, { emit }) {
    const { t } = useI18n();

    return () => modInfo.value?.signature && <NPopover trigger="hover">
      {{
        trigger: () => modInfo.value?.signature?.status === VerifyStatus.Valid ?
          <div class="text-green-5 i-tabler:certificate text-2em" />
          : <div class="text-red-5 i-tabler:certificate-off text-2em" />,
        default: () => <NFlex vertical>
          {modInfo.value?.signature?.status === VerifyStatus.Valid && modInfo.value.signature?.keyId === PubKeyId.Local &&
            <div>{t('mod.signature.verifiedOfficial')}</div>}
          {modInfo.value?.signature?.status === VerifyStatus.Valid && modInfo.value.signature?.keyId === PubKeyId.CI &&
            <div>{t('mod.signature.verifiedCI')}</div>}
          {modInfo.value?.signature?.status === VerifyStatus.NotFound &&
            <div>{t('mod.signature.notFound')}</div>}
          {modInfo.value?.signature?.status === VerifyStatus.InvalidSignature &&
            <div>{t('mod.signature.invalid')}</div>}
        </NFlex>
      }}
    </NPopover>;
  },
});
