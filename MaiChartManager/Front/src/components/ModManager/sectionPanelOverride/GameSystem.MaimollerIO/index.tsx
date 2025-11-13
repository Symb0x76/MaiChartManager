import api from '@/client/api';
import { t } from '@/locales';
import { globalCapture, modInfo, updateModInfo } from '@/store/refs';
import { useAsyncState } from '@vueuse/core';
import { NFlex, NButton } from 'naive-ui';
import { defineComponent, PropType, ref, computed, watch } from 'vue';

export default defineComponent({
  // props: {
  // },
  setup(props, { emit }) {
    const install = useAsyncState(async () => {
      await api.InstallMmlLibs();
      await updateModInfo();
    }, undefined, {
      immediate: false,
      onError(e) {
        globalCapture(e, t('mod.mmlIo.installFailed'));
      },
    })

    return () => <div class="">
      {!modInfo.value?.isMmlLibInstalled ? <NFlex align="center" class="m-l-35">
        <span class="c-orange">{t('mod.mmlIo.notInstalled')}</span>
        <NButton secondary onClick={() => install.execute()} loading={install.isLoading.value}>{t('mod.mmlIo.oneClickInstall')}</NButton>
      </NFlex>
        : <NFlex align="center" class="m-l-35">
          <span class="c-green-6">{t('mod.mmlIo.installed')}</span>
        </NFlex>}
    </div>;
  },
});
