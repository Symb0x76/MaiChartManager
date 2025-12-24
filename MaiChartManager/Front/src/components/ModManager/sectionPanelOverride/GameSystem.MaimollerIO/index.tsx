import api from '@/client/api';
import { Entry, IEntryState, ISectionState, Section } from '@/client/apiGen';
import { t } from '@/locales';
import { globalCapture, modInfo, updateModInfo } from '@/store/refs';
import { useAsyncState } from '@vueuse/core';
import { NFlex, NButton, NGrid, NGridItem, NFormItem, NSelect, NSwitch } from 'naive-ui';
import { defineComponent, PropType, ref, computed, watch } from 'vue';

export default defineComponent({
  props: {
    section: { type: Object as PropType<Section>, required: true },
    entryStates: {type: Object as PropType<Record<string, IEntryState>>, required: true},
    sectionState: {type: Object as PropType<ISectionState>, required: true},
  },
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

    const options = [
      { label: t('mod.ioKeyMap.disabled'), value: 'None' },
      { label: t('mod.ioKeyMap.select'), value: 'Select' },
      { label: t('mod.ioKeyMap.select1P'), value: 'Select1P' },
      { label: t('mod.ioKeyMap.select2P'), value: 'Select2P' },
      { label: t('mod.ioKeyMap.service'), value: 'Service' },
      { label: t('mod.ioKeyMap.test'), value: 'Test' },
    ];

    const PREFIX = 'GameSystem.MaimollerIO.';

    return () => <NFlex vertical>
      {!modInfo.value?.isMmlLibInstalled ? <NFlex align="center" class="m-l-35">
        <span class="c-orange">{t('mod.mmlIo.notInstalled')}</span>
        <NButton secondary onClick={() => install.execute()} loading={install.isLoading.value}>{t('mod.mmlIo.oneClickInstall')}</NButton>
      </NFlex>
        : <NFlex align="center" class="m-l-35">
          <span class="c-green-6">{t('mod.mmlIo.installed')}</span>
        </NFlex>}
      <div class="m-l-35">
        {t('mod.mmlIo.tip')}
      </div>
      <NGrid cols="1 500:2" yGap="12px">
        <NGridItem>
          <NFlex vertical>
            {props.section.entries?.filter(it=>
              ['P1', 'Touch1p', 'Button1p', 'Led1p'].map(it=>PREFIX + it).includes(it.path!))
              .map((entry) => {
                return <NFormItem label={t(`mod.mmlIo.${entry.path!.split('.').pop()}`)} labelPlacement="left" labelWidth="10em" showFeedback={false}>
                  <div class="flex h-34px items-center">
                    <NSwitch v-model:value={props.entryStates[entry.path!].value}/>
                  </div>
                </NFormItem>
              })
            }
            <NFormItem label={t('mod.mmlIo.button1')} labelPlacement="left" labelWidth="10em" showFeedback={false}>
              <NFlex vertical class="w-full ws-pre-line">
                <NSelect v-model:value={props.entryStates[PREFIX + 'Button1'].value} options={options}/>
              </NFlex>
            </NFormItem>
            <NFormItem label={t('mod.mmlIo.button2')} labelPlacement="left" labelWidth="10em" showFeedback={false}>
              <NFlex vertical class="w-full ws-pre-line">
                <NSelect v-model:value={props.entryStates[PREFIX + 'Button2'].value} options={options}/>
              </NFlex>
            </NFormItem>
          </NFlex>
        </NGridItem>
        <NGridItem>
          <NFlex vertical>
            {props.section.entries?.filter(it=>
              ['P2', 'Touch2p', 'Button2p', 'Led2p'].map(it=>PREFIX + it).includes(it.path!))
              .map((entry) => {
                return <NFormItem label={t(`mod.mmlIo.${entry.path!.split('.').pop()}`)} labelPlacement="left" labelWidth="10em" showFeedback={false}>
                  <div class="flex h-34px items-center">
                    <NSwitch v-model:value={props.entryStates[entry.path!].value}/>
                  </div>
                </NFormItem>
              })
            }
            <NFormItem label={t('mod.mmlIo.button3')} labelPlacement="left" labelWidth="10em" showFeedback={false}>
              <NFlex vertical class="w-full ws-pre-line">
                <NSelect v-model:value={props.entryStates[PREFIX + 'Button3'].value} options={options}/>
              </NFlex>
            </NFormItem>
            <NFormItem label={t('mod.mmlIo.button4')} labelPlacement="left" labelWidth="10em" showFeedback={false}>
              <NFlex vertical class="w-full ws-pre-line">
                <NSelect v-model:value={props.entryStates[PREFIX + 'Button4'].value} options={options}/>
              </NFlex>
            </NFormItem>
          </NFlex>
        </NGridItem>
      </NGrid>
    </NFlex>;
  },
});
